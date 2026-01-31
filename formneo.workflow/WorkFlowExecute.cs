using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLayer.Core.Services;
using NLayer.Service.Services;
using System.Linq;
using System.Net.Mail;
using System.Net;
using formneo.core.DTOs;
using formneo.core.Models;
using formneo.core.Operations;
using formneo.core.Services;
using formneo.service.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using formneo.core.DTOs.Budget.SF;
using formneo.workflow.NodeCompletionHandlers;

namespace formneo.workflow
{
    /// <summary>
    /// Refactored WorkFlowExecute - Clean Architecture
    /// Her metod tek bir sorumluluğa sahip, test edilebilir ve anlaşılır
    /// Strategy Pattern ile node completion handling
    /// </summary>
    public class WorkFlowExecute
    {
        private WorkFlowParameters _parameters;
        private readonly NodeCompletionHandlerFactory _completionHandlerFactory;

        public WorkFlowExecute()
        {
            // Factory'yi initialize et
            _completionHandlerFactory = new NodeCompletionHandlerFactory();
        }

        #region Public API

        /// <summary>
        /// Workflow başlatma veya devam ettirme (Orchestrator)
        /// </summary>
        public async Task<WorkflowHead> StartAsync(WorkFlowDto dto, WorkFlowParameters parameters, string payloadJson)
        {
            _parameters = parameters;

            // 1. WorkFlowDefination yükle ve parse et
            var workFlowDefination = await LoadWorkflowDefinitionAsync(dto.WorkFlowDefinationId);
            var workflow = ParseWorkflow(workFlowDefination, parameters);
            await workflow.PreloadAssignmentsAsync();

            // 2. İki farklı akış: CONTINUE veya START
            if (!string.IsNullOrEmpty(dto.WorkFlowId))
            {
                // CONTINUE: Mevcut workflow'u devam ettir
                return await ContinueWorkflowAsync(dto, workflow, workFlowDefination, payloadJson);
            }
            else
            {
                // START: Yeni workflow başlat
                return await StartNewWorkflowAsync(dto, workflow, workFlowDefination, payloadJson);
            }
        }

        #endregion

        #region Continue Workflow

        /// <summary>
        /// Mevcut workflow'u devam ettir (Continue akışı)
        /// Kullanıcı formu doldurdu veya onay verdi
        /// </summary>
        private async Task<WorkflowHead> ContinueWorkflowAsync(
            WorkFlowDto dto,
            Workflow workflow,
            WorkFlowDefinationDto workFlowDefination,
            string payloadJson)
        {
            // 1. Mevcut workflow'u yükle (DB'den)
            var head = await LoadExistingWorkflowAsync(dto.WorkFlowId);
            workflow._workFlowItems = head.workflowItems;
            workflow._HeadId = head.Id;

            // 2. Hangi node'u tamamladığını bul
            var startNode = FindWorkflowNode(head, dto.NodeId);

            // 3. ApproverNode ise: Onayı güncelle
            ApproveItems approverItem = null;
            if (!string.IsNullOrEmpty(dto.ApproverItemId))
            {
                // ✅ ApproveItem.Status = Approved/Rejected yapılıyor
                approverItem = await UpdateApproverItemAsync(dto);
            }

            // 4. Workflow engine'i çalıştır (node'ları execute et)
            string action = dto.Action ?? "";
            await workflow.Continue(startNode, startNode.NodeId, dto.UserName, action, head, null, payloadJson);

            // 5. Node tamamlama işlemlerini handler'a devret (Strategy Pattern)
            var completionResult = await ProcessNodeCompletionAsync(startNode, dto, payloadJson, head.Id);

            // 6. Tüm değişiklikleri veritabanına kaydet
            await _parameters.workFlowService.UpdateWorkFlowAndRelations(
                head, 
                head.workflowItems, 
                approverItem, 
                completionResult.FormItem, 
                completionResult.FormInstance);

            // 7. Mail gönder (sonraki onayçılara veya reddedilenlere)
            await SendNotificationEmailsAsync(head);

            return null; // Continue response'u null döner (Controller handle eder)
        }

        #endregion

        #region Start Workflow

        /// <summary>
        /// Yeni workflow başlat (Start akışı)
        /// </summary>
        private async Task<WorkflowHead> StartNewWorkflowAsync(
            WorkFlowDto dto,
            Workflow workflow,
            WorkFlowDefinationDto workFlowDefination,
            string payloadJson)
        {
            // 1. WorkflowHead oluştur
            var head = CreateWorkflowHead(dto, workFlowDefination);

            // 2. Workflow'u başlat
            string action = dto.Action ?? "";
            await workflow.Start(dto.UserName, payloadJson, action);
            head.workflowItems = workflow._workFlowItems;

            // 3. AlertNode kontrolü (varsa rollback)
            var pendingAlertNode = head.workflowItems.FirstOrDefault(item =>
                item.NodeType == "alertNode" && item.workFlowNodeStatus == WorkflowStatus.Pending);

            if (pendingAlertNode != null)
            {
                return CreateRollbackResponse(head);
            }

            // 4. Navigation property'leri temizle (EF Core threading fix)
            CleanNavigationProperties(head);

            // 5. Veritabanına kaydet
            var result = await _parameters.workFlowService.AddAsync(head);
            if (result == null) return null;

            // 6. ✅ START ÖZEL: Eğer form verisi varsa FormInstance oluştur
            if (!string.IsNullOrEmpty(payloadJson))
            {
                await ProcessFormInstanceForStartAsync(result, dto, payloadJson);
            }

            // 7. Mail gönder (pending ApproveItem'lar için)
            await SendNotificationEmailsAsync(result);

            return result;
        }
    
        #endregion

        #region Helper Methods - Load & Parse

        private async Task<WorkFlowDefinationDto> LoadWorkflowDefinitionAsync(Guid definitionId)
        {
            var response = await _parameters._workFlowDefination.GetByIdGuidAsync(definitionId);
            if (response?.Data == null)
            {
                throw new Exception($"WorkFlowDefination with id '{definitionId}' not found");
            }
            return response.Data;
        }

        private Workflow ParseWorkflow(WorkFlowDefinationDto definition, WorkFlowParameters parameters)
        {
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var workflow = JsonConvert.DeserializeObject<Workflow>(definition.Defination!, settings);
            workflow._parameters = parameters;
            
            // Edge'leri temizle (geçersiz node'ları filtrele)
            var nodeIds = new HashSet<string>(workflow.Nodes.Select(n => n.Id));
            workflow.Edges = workflow.Edges
                .Where(e => nodeIds.Contains(e.Source) && nodeIds.Contains(e.Target))
                .ToList();

            workflow._workFlowItems = new List<WorkflowItem>();

            return workflow;
        }

        private async Task<WorkflowHead> LoadExistingWorkflowAsync(string workflowId)
        {
            if (string.IsNullOrEmpty(workflowId))
            {
                throw new ArgumentException("WorkFlowId is required for continuing workflow");
            }

            if (!Guid.TryParse(workflowId, out Guid guid))
            {
                throw new ArgumentException($"Invalid WorkFlowId format: {workflowId}");
            }

            var head = await _parameters.workFlowService.GetWorkFlowWitId(guid);
            if (head == null)
            {
                throw new Exception($"WorkflowHead with id '{workflowId}' not found");
            }

            return head;
        }

        private WorkflowItem FindWorkflowNode(WorkflowHead head, string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentException("NodeId is required for continuing workflow");
            }

            if (!Guid.TryParse(nodeId, out Guid guid))
            {
                throw new ArgumentException($"Invalid NodeId format: {nodeId}");
            }

            var node = head.workflowItems.FirstOrDefault(e => e.Id == guid);
            if (node == null)
            {
                throw new Exception($"WorkflowItem with id '{nodeId}' not found");
            }

            return node;
        }

        #endregion

        #region Helper Methods - ApproverItem

        private async Task<ApproveItems> UpdateApproverItemAsync(WorkFlowDto dto)
        {
            var approverItem = await _parameters._approverItemsService
                .GetByIdStringGuidAsync(new Guid(dto.ApproverItemId));

            if (approverItem == null) return null;

            // Action'a göre status güncelle
            if (!string.IsNullOrEmpty(dto.Action))
            {
                approverItem.ApproverStatus = ParseApprovalAction(dto.Action);
            }

            // Note & NumberManDay
            if (dto.Note != null)
                approverItem.ApprovedUser_RuntimeNote = dto.Note;
            if (dto.NumberManDay != null)
                approverItem.ApprovedUser_RuntimeNumberManDay = dto.NumberManDay;

            // Runtime user bilgileri
            var utils = new Utils();
            approverItem.ApprovedUser_RuntimeNameSurname = utils.GetNameAndSurnameAsync(dto.UserName);
            approverItem.ApprovedUser_RuntimeId = await ResolveUserIdAsync(dto.UserName);

            return approverItem;
        }

        private ApproverStatus ParseApprovalAction(string action)
        {
            var actionUpper = action.ToUpper();
            if (actionUpper == "APPROVE" || actionUpper == "ONAYLA" || actionUpper == "YES")
                return ApproverStatus.Approve;
            else if (actionUpper == "REJECT" || actionUpper == "REDDET" || actionUpper == "NO")
                return ApproverStatus.Reject;

            return ApproverStatus.Pending;
        }

        #endregion

        #region Helper Methods - Node Completion

        /// <summary>
        /// Node tamamlama işlemlerini handler'a devret
        /// Strategy Pattern: Her node tipi için ayrı handler
        /// </summary>
        private async Task<NodeCompletionResult> ProcessNodeCompletionAsync(
            WorkflowItem node,
            WorkFlowDto dto,
            string payloadJson,
            Guid workflowHeadId)
        {
            // Factory'den doğru handler'ı al
            var handler = _completionHandlerFactory.GetHandler(node.NodeType);

            // Context oluştur
            var context = new NodeCompletionContext
            {
                Node = node,
                Dto = dto,
                PayloadJson = payloadJson,
                WorkflowHeadId = workflowHeadId,
                Parameters = _parameters
            };

            // Handler'a devret
            return await handler.HandleAsync(context);
        }

        /// <summary>
        /// START ile form verisi geldiğinde FormInstance oluştur
        /// BASIT: Direkt INSERT, handler çağırmaya gerek yok
        /// </summary>
        private async Task ProcessFormInstanceForStartAsync(
            WorkflowHead head,
            WorkFlowDto dto,
            string payloadJson)
        {
            // 1. FormTaskNode bul
            var formTask = head.workflowItems.FirstOrDefault(wi => 
                wi.NodeType == "formTaskNode" &&
                wi.formItems != null && 
                wi.formItems.Any());
            
            if (formTask == null) return;
            
            // 2. İlk FormItem'ı al
            var formItem = formTask.formItems.First();
            
            // 3. FormDesign yükle (gerekirse)
            if (string.IsNullOrEmpty(formItem.FormDesign) && formItem.FormId.HasValue)
                    {
                        try
                        {
                    var form = await _parameters._formService.GetByIdStringGuidAsync(formItem.FormId.Value);
                            if (form != null && !string.IsNullOrEmpty(form.FormDesign))
                    {
                        formItem.FormDesign = form.FormDesign;
                    }
                }
                catch { }
            }
            
            // 4. ✅ Direkt FormInstance oluştur (BASIT!)
            var utils = new Utils();
            var formInstance = new FormInstance
            {
                WorkflowHeadId = head.Id,
                FormId = formItem.FormId,
                FormDesign = formItem.FormDesign,
                FormData = payloadJson,
                UpdatedBy = dto.UserName,
                UpdatedByNameSurname = utils.GetNameAndSurnameAsync(dto.UserName).ToString()
            };
            
            // 5. ✅ Direkt INSERT
            await _parameters._formInstanceService.AddAsync(formInstance);
        }

        #endregion

        #region Helper Methods - WorkflowHead

        private WorkflowHead CreateWorkflowHead(WorkFlowDto dto, WorkFlowDefinationDto definition)
        {
            return new WorkflowHead
            {
                WorkflowName = definition.WorkflowName,
                CreateUser = dto.UserName,
                WorkFlowInfo = dto.WorkFlowInfo,
                WorkFlowDefinationId = dto.WorkFlowDefinationId,
                WorkFlowDefinationJson = definition.Defination,
                workFlowStatus = WorkflowStatus.InProgress
            };
        }

        private WorkflowHead CreateRollbackResponse(WorkflowHead head)
        {
                    head.Id = Guid.Empty; // Rollback flag
                    head.workFlowStatus = WorkflowStatus.Pending;
            return head;
        }

        private void CleanNavigationProperties(WorkflowHead head)
        {
                foreach (var item in head.workflowItems)
                {
                    if (item.formItems != null)
                    {
                        foreach (var formItem in item.formItems)
                        {
                        formItem.FormUser = null;
                        }
                    }
                    if (item.approveItems != null)
                    {
                        foreach (var approveItem in item.approveItems)
                        {
                        approveItem.ApproveUser = null;
                        approveItem.ApprovedUser_Runtime = null;
                    }
                }
            }
        }

        #endregion

        #region Helper Methods - Email

        private async Task SendNotificationEmailsAsync(WorkflowHead head)
        {
            if (_parameters?.UserManager == null) return;

            // Workflow tamamlandıysa
            if (head.workFlowStatus == WorkflowStatus.Completed)
            {
                SendMail(MailStatus.OnaySureciTamamlandı, head.CreateUser, "", head.UniqNumber.ToString());
            }

            // Pending veya Rejected ApproveItem'lar için
                    foreach (var item in head.workflowItems)
                    {
                if (item.approveItems == null) continue;

                            foreach (var mail in item.approveItems)
                            {
                                if (mail.ApproverStatus == ApproverStatus.Pending)
                                {
                        var email = await GetUserEmailAsync(mail.ApproveUserId);
                        SendMail(MailStatus.OnayınızaSunuldu, email, mail.ApproveUserNameSurname,
                            head.UniqNumber.ToString(), head.WorkFlowInfo);
                    }
                    else if (mail.ApproverStatus == ApproverStatus.Reject)
                    {
                        var email = await GetUserEmailAsync(mail.ApprovedUser_RuntimeId);
                        SendMail(MailStatus.Reddedildi, email, mail.ApproveUserNameSurname,
                            head.UniqNumber.ToString(), head.WorkFlowInfo);
                    }
                }
            }
        }

        private async Task<string> GetUserEmailAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return "";

            try
            {
                var user = await _parameters.UserManager.FindByIdAsync(userId);
                return user?.Email ?? user?.UserName ?? userId;
            }
            catch
            {
                return userId;
            }
        }

        private void SendMail(MailStatus status, string UserName, string text = "", string approveId = "", string workFlowInfo = "")
        {
            // E-posta gönderen bilgileri
            string senderEmail = "supportformneo@formneo-tech.com";
            string senderPassword = "supportformneo.2024";

            string subject = "Onay Süreci Bilgilendirmesi";
            string kullaniciAdi = "";
            string onayDurumu = "";

            if (status == MailStatus.Onaylandi)
            {
                kullaniciAdi = UserName;
                onayDurumu = "Onaylandı";
            }

            if (status == MailStatus.OnayınızaSunuldu)
            {
                kullaniciAdi = UserName;
                onayDurumu = "Onayınıza Sunuldu";
            }

            if (status == MailStatus.Reddedildi)
            {
                kullaniciAdi = UserName;
                onayDurumu = "Reddedildi";
            }

            if (status == MailStatus.OnaySureciBasladi)
            {
                kullaniciAdi = UserName;
                onayDurumu = "Onay Süreci Başladı";
            }

            if (status == MailStatus.OnaySureciTamamlandı)
            {
                kullaniciAdi = UserName;
                onayDurumu = "İş Akışı Süreci Tamamlandı";
            }

            // HTML şablonu
            string body = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9; }}
        h1 {{ color: #0056b3; }}
        .details {{ background-color: #fff; padding: 15px; border-radius: 5px; border: 1px solid #ddd; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Onay Süreci Bilgilendirmesi</h1>
        <p>Sayın <strong>{kullaniciAdi}</strong>,</p>
        <p>Aşağıda belirtilen sürece ait onay işlemi hakkında bilgilendirme:</p>

        <div class='details'>
            <p><strong>Onay ID:</strong> {approveId}</p>
            <p><strong>Durum:</strong> <span style='color:{(onayDurumu == "Onaylandı" ? "green" : "red")};'>{onayDurumu}</span></p>
            <p><strong>Süreç Detayı: {text}</strong></p>
        </div>

        <p>Gerekli aksiyonları zamanında almanızı rica ederiz.</p>
        <p style=""color:#0056b3;""><strong>formneo Destek Sistemi: https://support.formneo-tech.com/</strong></p>
        
        <div class='footer'>
            <p>Bu e-posta otomatik olarak oluşturulmuştur, lütfen yanıtlamayınız.</p>
            <p><strong>formneo</strong></p>
        </div>
    </div>
</body>
</html>";

            // SMTP ayarları
            SmtpClient smtpClient = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            // E-posta oluştur
            MailMessage mail = new MailMessage(senderEmail, UserName, subject, body)
            {
                IsBodyHtml = true
            };

            try
            {
                // E-postayı gönder
                smtpClient.Send(mail);
                Console.WriteLine("E-posta başarıyla gönderildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("E-posta gönderme sırasında bir hata oluştu: " + ex.Message);
            }
        }

        #endregion

        #region Helper Methods - User

        private async Task<string> ResolveUserIdAsync(string userName)
        {
            if (_parameters?.UserManager == null || string.IsNullOrEmpty(userName))
                return userName;

            try
            {
                var user = await _parameters.UserManager.FindByNameAsync(userName);
                return user?.Id ?? userName;
            }
            catch
            {
                return userName;
            }
        }

        #endregion

        #region Enums

        public enum MailStatus
        {
            Onaylandi = 1,
            Reddedildi = 2,
            OnayınızaSunuldu = 3,
            OnaySureciTamamlandı = 4,
            OnaySureciBasladi = 5
        }

        #endregion
    }
}

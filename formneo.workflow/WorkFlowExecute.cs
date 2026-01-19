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

namespace formneo.workflow
{
    public class WorkFlowExecute
    {
        WorkFlowParameters _parameters;
        public async Task<WorkflowHead> StartAsync(WorkFlowDto dto, WorkFlowParameters parameters, string payloadJson)
        {

            _parameters = parameters;




            List<WorkflowItem> workFlowItems = new List<WorkflowItem>();

            WorkflowHead head = new WorkflowHead();
            var workFlowDefination = await parameters._workFlowDefination.GetByIdGuidAsync(new Guid(dto.WorkFlowDefinationId.ToString()));

            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };


            if (workFlowDefination == null)
            {



            }
            Workflow workflow = JsonConvert.DeserializeObject<Workflow>(workFlowDefination!.Data.Defination!, settings);

            workflow._parameters = parameters;

            HashSet<string> nodeIds = new HashSet<string>(workflow.Nodes.Select(node => node.Id));

            workflow.Edges = workflow.Edges.Where(edge => nodeIds.Contains(edge.Source) && nodeIds.Contains(edge.Target)).ToList();
            workflow._workFlowItems = new List<WorkflowItem>();

            head.WorkFlowInfo = dto.WorkFlowInfo;

            head.WorkFlowDefinationJson = workFlowDefination!.Data.Defination!;
            //Daha önce var devam et
            if (!String.IsNullOrEmpty(dto.WorkFlowId))
            {


                /// Onay Kaydı Bul
                /// 

                var ApproverItem = await _parameters._approverItemsService.GetByIdStringGuidAsync(new Guid(dto.ApproverItemId));

                // Buton bazlı sistem: Action'a göre durum belirlenir (APPROVE, REJECT, vb.)
                // Artık Input ile "yes/no" mantığı yok
                if (!string.IsNullOrEmpty(dto.Action))
                {
                    string actionUpper = dto.Action.ToUpper();
                    if (actionUpper == "APPROVE" || actionUpper == "ONAYLA" || actionUpper == "YES")
                    {
                        ApproverItem.ApproverStatus = ApproverStatus.Approve;
                    }
                    else if (actionUpper == "REJECT" || actionUpper == "REDDET" || actionUpper == "NO")
                    {
                        ApproverItem.ApproverStatus = ApproverStatus.Reject;
                    }
                }

                if (dto.Note != null)
                {
                    ApproverItem.ApprovedUser_RuntimeNote = dto.Note;
                }
                if (dto.NumberManDay != null)
                {
                    ApproverItem.ApprovedUser_RuntimeNumberManDay = dto.NumberManDay;
                }

                Utils utils = new Utils();

                var approverUser = utils.GetNameAndSurnameAsync(dto.UserName);
                ApproverItem.ApprovedUser_RuntimeNameSurname = approverUser;

                ApproverItem.ApprovedUser_Runtime = dto.UserName;


                Guid g = new Guid(dto.WorkFlowId);
                head = await parameters.workFlowService.GetWorkFlowWitId(g);
                workflow._workFlowItems = head.workflowItems;
                var startNode = head.workflowItems.Where(e => e.Id == new Guid(dto.NodeId)).FirstOrDefault();

                workflow._HeadId = new Guid(dto.WorkFlowId);

                // Continue metoduna payloadJson'ı da geç (FormData için)
                // Artık Input yerine Action kullanılıyor (buton bazlı sistem)
                string actionToPass = dto.Action ?? "";
                await workflow.Continue(startNode, startNode.NodeId, dto.UserName, actionToPass, head, null, payloadJson);

                // FormInstance güncellemesi sadece FormTaskNode'dan geliyorsa yapılır
                // ApproverNode (UserTask)'dan geliyorsa FormInstance güncellenmez
                FormInstance formInstanceToSave = null;
                FormItems formItemToSave = null;
                
                // Hangi node'dan geldiğini kontrol et
                // Sadece FormTaskNode'dan geliyorsa FormInstance işlemleri yapılır
                // ApproverNode (UserTask)'dan geliyorsa FormInstance güncellenmez
                if (startNode.NodeType == "formTaskNode")
                {
                    // FormItems'ı bul ve kaydet (sadece FormTaskNode için)
                    WorkflowItem formTaskWorkflowItem = null;
                    foreach (var item in head.workflowItems)
                    {
                        // FormTaskNode kontrolü - sadece formTaskNode için FormInstance güncelle
                        if (item.NodeType == "formTaskNode" && item.formItems != null && item.formItems.Count > 0)
                        {
                            formItemToSave = item.formItems.FirstOrDefault();
                            formTaskWorkflowItem = item;
                            // Kullanıcı mesajını ekle
                            if (formItemToSave != null && dto.Note != null)
                            {
                                formItemToSave.FormUserMessage = dto.Note;
                            }
                            // Form verilerini kaydet: payloadJson (FormTaskNode'dan geliyorsa FormData gelir)
                            if (formItemToSave != null && !string.IsNullOrEmpty(payloadJson))
                            {
                                formItemToSave.FormData = payloadJson;
                            }
                            break;
                        }
                    }

                    // FormInstance'ı güncelle veya oluştur (her zaman son/güncel form verisi)
                    // Sadece FormTaskNode tamamlandığında FormInstance güncellenir
                    if (formTaskWorkflowItem != null && formItemToSave != null && !string.IsNullOrEmpty(payloadJson))
                {
                    // FormDesign'i belirle: FormItem'dan veya FormId varsa Form tablosundan
                    string formDesign = formItemToSave.FormDesign;
                    
                    // Eğer FormDesign boşsa ve FormId varsa, Form tablosundan al
                    if (string.IsNullOrEmpty(formDesign) && formItemToSave.FormId.HasValue && parameters._formService != null)
                    {
                        try
                        {
                            var form = await parameters._formService.GetByIdStringGuidAsync(formItemToSave.FormId.Value);
                            if (form != null && !string.IsNullOrEmpty(form.FormDesign))
                            {
                                formDesign = form.FormDesign;
                                // FormItem'daki FormDesign'i de güncelle
                                formItemToSave.FormDesign = formDesign;
                            }
                        }
                        catch
                        {
                            // Form bulunamazsa devam et
                        }
                    }
                    
                    // Mevcut FormInstance'ı kontrol et
                    var existingFormInstanceQuery = parameters._formInstanceService.Where(e => e.WorkflowHeadId == head.Id);
                    var existingFormInstance = await existingFormInstanceQuery.FirstOrDefaultAsync();

                    string userNameSurname = utils.GetNameAndSurnameAsync(dto.UserName).ToString();

                    if (existingFormInstance != null)
                    {
                        // Mevcut FormInstance'ı güncelle
                        existingFormInstance.FormData = payloadJson; // FormData (payloadJson)
                        existingFormInstance.FormDesign = formDesign; // Güncellenmiş FormDesign'i kullan
                        existingFormInstance.FormId = formItemToSave.FormId;
                        existingFormInstance.UpdatedBy = dto.UserName;
                        existingFormInstance.UpdatedByNameSurname = userNameSurname;
                        existingFormInstance.UpdatedDate = DateTime.Now;
                        formInstanceToSave = existingFormInstance;
                    }
                    else
                    {
                        // Yeni FormInstance oluştur
                        formInstanceToSave = new FormInstance
                        {
                            WorkflowHeadId = head.Id,
                            FormId = formItemToSave.FormId,
                            FormDesign = formDesign, // Güncellenmiş FormDesign'i kullan
                            FormData = payloadJson, // FormData (payloadJson)
                            UpdatedBy = dto.UserName,
                            UpdatedByNameSurname = userNameSurname
                        };
                    }
                    }
                }

                // Continue metodunda head.workflowItems'i gönder (FormItems'ları kaydetmek için)
                var result = await parameters.workFlowService.UpdateWorkFlowAndRelations(head, head.workflowItems, ApproverItem, formItemToSave, formInstanceToSave);

                if (result != null)
                {

                    if (head.workFlowStatus == WorkflowStatus.Completed)
                    {
                        string getUniqApproveId = head.UniqNumber.ToString();

                        SendMail(MailStatus.OnaySureciTamamlandı, head.CreateUser, "", getUniqApproveId);
                    }

                    foreach (var item in head.workflowItems)
                    {



                        if (item.approveItems != null)
                        {
                            foreach (var mail in item.approveItems!)
                            {




                                //string getUniqApproveId = Utils.ShortenGuid(head.Id);


                                if (mail.ApproverStatus == ApproverStatus.Pending)
                                {
                                    SendMail(MailStatus.OnayınızaSunuldu, mail.ApproveUser, mail.ApproveUserNameSurname, head.UniqNumber.ToString(), head.WorkFlowInfo);
                                }
                                if (mail.ApproverStatus == ApproverStatus.Reject)
                                {


                                    SendMail(MailStatus.Reddedildi, mail.ApprovedUser_Runtime, mail.ApproveUserNameSurname, head.UniqNumber.ToString(), head.WorkFlowInfo);
                                }
                            }
                        }
                    }
                }
                return null;
            }
            else
            {

                head = new WorkflowHead();
                head.WorkflowName = workFlowDefination.Data.WorkflowName;
                head.CreateUser = dto.UserName;
                head.WorkFlowInfo = dto.WorkFlowInfo;
                head.WorkFlowDefinationId = dto.WorkFlowDefinationId;

                head.WorkFlowDefinationJson = workFlowDefination!.Data.Defination!;
                head.workFlowStatus = WorkflowStatus.InProgress;

                // Action'ı workflow'a geçir
                // NOT: Action Start metodunda set edilir ve formNode'a gelince kullanılır
                string actionToPass = dto.Action ?? "";
                await workflow.Start(dto.UserName, payloadJson, actionToPass);

                head.workflowItems = workflow._workFlowItems;

                // AlertNode kontrolü - AlertNode'a gelirse rollback yapılacak
                // AlertNode sadece error ve warning için kullanılır, success ve info mesajları normal component'te gösterilir
                // AlertNode'a gelince işlem durdurulur ve rollback yapılır
                var pendingAlertNode = head.workflowItems.FirstOrDefault(item => 
                    item.NodeType == "alertNode" && item.workFlowNodeStatus == WorkflowStatus.Pending);
                
                // Eğer alertNode'a gelindi ise, rollback yap (işlemi geri al)
                if (pendingAlertNode != null)
                {
                    // Rollback: WorkflowHead ve WorkflowItem'ları kaydetme
                    // Alert bilgilerini response'da döndürmek için head'i işaretle
                    // Id'yi Empty yap = rollback flag (response builder bunu algılayacak)
                    head.Id = Guid.Empty; // Rollback flag
                    head.workFlowStatus = WorkflowStatus.Pending;
                    return head; // Alert bilgileriyle birlikte döndür, ama veritabanına kaydetme
                }

                var result = await parameters.workFlowService.AddAsync(head);

                // FormItems'ları kaydet (UpdateWorkFlowAndRelations ile - ApproveItems mantığı gibi)
                // FormTaskNode'a gelindiğinde FormItems oluşturuldu ve workFlowItem.formItems'e eklendi
                // UpdateWorkFlowAndRelations içinde FormItems'lar kaydedilecek
                
                // FormTaskNode için FormInstance oluştur (ilk kez oluşturulduğunda)
                FormItems formItemToSave = null;
                FormInstance formInstanceToSave = null;
                
                if (result != null && result.workflowItems != null)
                {
                    // FormTaskNode kontrolü - FormTaskNode'a gelindiğinde FormInstance oluştur
                    foreach (var item in result.workflowItems)
                    {
                        if (item.NodeType == "formTaskNode" && item.formItems != null && item.formItems.Count > 0)
                        {
                            formItemToSave = item.formItems.FirstOrDefault();
                            if (formItemToSave != null)
                            {
                                // FormDesign'i belirle: FormItem'dan veya FormId varsa Form tablosundan
                                string formDesign = formItemToSave.FormDesign;
                                
                                // Eğer FormDesign boşsa ve FormId varsa, Form tablosundan al
                                // Bu işlem async olduğu için Start metodunda yapılır (ExecuteFormTaskNode senkron)
                                if (string.IsNullOrEmpty(formDesign) && formItemToSave.FormId.HasValue && parameters._formService != null)
                                {
                                    try
                                    {
                                        var form = await parameters._formService.GetByIdStringGuidAsync(formItemToSave.FormId.Value);
                                        if (form != null && !string.IsNullOrEmpty(form.FormDesign))
                                        {
                                            formDesign = form.FormDesign;
                                            // FormItem'daki FormDesign'i de güncelle
                                            formItemToSave.FormDesign = formDesign;
                                        }
                                    }
                                    catch
                                    {
                                        // Form bulunamazsa devam et
                                    }
                                }
                                
                                // FormInstance oluştur (FormDesign boş olsa bile oluştur, sonra güncellenebilir)
                                Utils utils = new Utils();
                                string userNameSurname = utils.GetNameAndSurnameAsync(dto.UserName).ToString();
                                
                                // FormData her zaman gelir (formdan butonla action ve form verisi gelir)
                                // FormItem'a da FormData'yı kaydet
                                if (!string.IsNullOrEmpty(payloadJson))
                                {
                                    formItemToSave.FormData = payloadJson;
                                }
                                
                                // FormInstance oluştur (FormDesign boş olsa bile, sonra güncellenebilir)
                                formInstanceToSave = new FormInstance
                                {
                                    WorkflowHeadId = result.Id,
                                    FormId = formItemToSave.FormId,
                                    FormDesign = formDesign, // FormDesign varsa kullan, yoksa null olabilir
                                    FormData = payloadJson, // FormData her zaman gelir
                                    UpdatedBy = dto.UserName,
                                    UpdatedByNameSurname = userNameSurname
                                };
                            }
                            break;
                        }
                    }
                    
                    await parameters.workFlowService.UpdateWorkFlowAndRelations(result, result.workflowItems, null, formItemToSave, formInstanceToSave);
                }

                if (result != null)
                {
                    foreach (var item in head.workflowItems)
                    {
                        foreach (var mail in item.approveItems!)
                        {
                            if (mail.ApproverStatus == ApproverStatus.Pending)
                            {


                                string getUniqApproveId = Utils.ShortenGuid(head.Id);
                                SendMail(MailStatus.OnayınızaSunuldu, mail.ApproveUser, mail.ApproveUserNameSurname, head.UniqNumber.ToString(), head.WorkFlowInfo);
                            }
                        }
                    }
                    
                    // Eğer workflow execution sırasında alertNode'a gelip pending durumunda kaldıysa
                    // (ama error/warning değilse, sadece info ise)
                    if (pendingAlertNode != null)
                    {
                        var workflowJson = JObject.Parse(head.WorkFlowDefinationJson);
                        var nodes = workflowJson["nodes"] as JArray;
                        var alertNodeDef = nodes?.FirstOrDefault(n => n["id"]?.ToString() == pendingAlertNode.NodeId);
                        var alertType = alertNodeDef?["data"]?["type"]?.ToString()?.ToLower() ?? "info";
                        
                        // Info tipindeki alert'ler için rollback yapma, sadece pending olarak işaretle
                        if (alertType == "info" || alertType == "success")
                        {
                            result.workFlowStatus = WorkflowStatus.Pending;
                            result.CurrentNodeId = pendingAlertNode.NodeId;
                        }
                    }
                }

                return result;
            }
        }

            
        // JSON verisini Workflow nesnesine dönüştüren metod
        private static Workflow ConvertJsonToWorkflow(string jsonData)
        {
            Workflow workflow = JsonConvert.DeserializeObject<Workflow>(jsonData.Replace(";", ""));
            return workflow;
        }

        // JSON verisi
        private static void SendMail(MailStatus status, string UserName, string SendApproverSurname, string approveId, string text = "")
        {

            string senderEmail = "support@formneo.com";
            string senderPassword = "Sifre2634@!!";

            // E-posta alıcısının adresi
            string toEmail = "murat.merdogan@formneo.com";

            // E-posta başlığı
            string subject = "Onay Süreci Hakkında Bilgilendirme";

            // Dinamik veriler
            string kullaniciAdi = "";
            string numaralar = "12345, 67890";
            string onayDurumu = "";
            if (status == MailStatus.OnayınızaSunuldu)
            {

                kullaniciAdi = SendApproverSurname;
                onayDurumu = "Onayınıza Sunuldu"; // veya "Onaylandı" / "Reddedildi"
            }

            if (status == MailStatus.Reddedildi)
            {
                kullaniciAdi = UserName;
                onayDurumu = "Reddedildi"; // veya "Onaylandı" / "Reddedildi"
            }

            if (status == MailStatus.OnaySureciBasladi)
            {

                kullaniciAdi = UserName;
                onayDurumu = "Onay Süreci Başladı"; // veya "Onaylandı" / "Reddedildi"
            }

            if (status == MailStatus.OnaySureciTamamlandı)
            {
                kullaniciAdi = UserName;
                onayDurumu = "İş Akışı Süreci Tamamlandı"; // veya "Onaylandı" / "Reddedildi"
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


            // SMTP (Simple Mail Transfer Protocol) ayarları
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

            //mail.To.Add(new MailAddress(UserName));

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


        public enum MailStatus
        {
            Onaylandi = 1,
            Reddedildi = 2,
            OnayınızaSunuldu = 3,
            OnaySureciTamamlandı = 4,
            OnaySureciBasladi = 5
        }
    }
}
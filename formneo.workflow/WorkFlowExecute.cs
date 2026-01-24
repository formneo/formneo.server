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




            // ✅ THREADING FIX: TÜM DbContext sorgularını en başta yap
            // 1. WorkFlowDefination yükle
            var workFlowDefination = await parameters._workFlowDefination.GetByIdGuidAsync(dto.WorkFlowDefinationId);
            
            if (workFlowDefination == null)
            {
                throw new Exception("WorkFlowDefination not found");
            }

            // 2. Workflow'u parse et
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            Workflow workflow = JsonConvert.DeserializeObject<Workflow>(workFlowDefination!.Data.Defination!, settings);
            workflow._parameters = parameters;
            
            // 3. EmployeeAssignments'i önceden yükle (cache'le)
            await workflow.PreloadAssignmentsAsync();
            
            // ✅ Artık hiçbir DbContext sorgusu kalmadı, workflow execution başlayabilir
            List<WorkflowItem> workFlowItems = new List<WorkflowItem>();
            WorkflowHead head = new WorkflowHead();

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

                ApproveItems ApproverItem = null;

                // ApproverItemId sadece ApproverNode (UserTask) için gerekli
                // FormTaskNode için null olabilir, bu durumda ApproverItem null kalır
                if (!string.IsNullOrEmpty(dto.ApproverItemId))
                {
                    ApproverItem = await _parameters._approverItemsService.GetByIdStringGuidAsync(new Guid(dto.ApproverItemId));

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

                    // dto.UserName'i UserId'ye çevir
                    string runtimeUserId = dto.UserName; // Default
                    if (_parameters?.UserManager != null && !string.IsNullOrEmpty(dto.UserName))
                    {
                        try
                        {
                            var user = await _parameters.UserManager.FindByNameAsync(dto.UserName);
                            if (user != null)
                            {
                                runtimeUserId = user.Id;
                            }
                        }
                        catch { }
                    }
                    ApproverItem.ApprovedUser_RuntimeId = runtimeUserId;
                }


                // WorkFlowId ve NodeId nullable string, bu yüzden kontrol gerekli
                if (string.IsNullOrEmpty(dto.WorkFlowId) || string.IsNullOrEmpty(dto.NodeId))
                {
                    throw new ArgumentException("WorkFlowId and NodeId are required for continuing workflow");
                }

                if (!Guid.TryParse(dto.WorkFlowId, out Guid workflowHeadGuid))
                {
                    throw new ArgumentException($"Invalid WorkFlowId format: {dto.WorkFlowId}");
                }

                if (!Guid.TryParse(dto.NodeId, out Guid nodeGuid))
                {
                    throw new ArgumentException($"Invalid NodeId format: {dto.NodeId}");
                }

                head = await parameters.workFlowService.GetWorkFlowWitId(workflowHeadGuid);
                workflow._workFlowItems = head.workflowItems;
                var startNode = head.workflowItems.Where(e => e.Id == nodeGuid).FirstOrDefault();

                workflow._HeadId = workflowHeadGuid;

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
                            formTaskWorkflowItem = item;
                            
                            // dto.UserName'e göre doğru FormItem'ı bul
                            // dto.UserName Identity.Name'den geliyor, UserId'ye çevir
                            string currentUserId = dto.UserName; // Default: UserName
                            
                            // UserManager varsa, UserName'den UserId'yi al
                            if (_parameters?.UserManager != null && !string.IsNullOrEmpty(dto.UserName))
                            {
                                try
                                {
                                    var user = await _parameters.UserManager.FindByNameAsync(dto.UserName);
                                    if (user != null)
                                    {
                                        currentUserId = user.Id;
                                    }
                                }
                                catch
                                {
                                    // Hata durumunda UserName kullan
                                }
                            }
                            
                            // Eğer birden fazla FormItem varsa (department_all gibi durumlar), dolduran kullanıcının FormItem'ını bul
                            formItemToSave = item.formItems.FirstOrDefault(fi => fi.FormUserId == currentUserId);
                            
                            // Eğer bulunamazsa (eski sistem uyumluluğu için), ilk FormItem'ı al
                            if (formItemToSave == null)
                            {
                                formItemToSave = item.formItems.FirstOrDefault();
                            }
                            
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
                            
                            // Dolduran kullanıcının FormItem'ını Completed yap
                            if (formItemToSave != null)
                            {
                                formItemToSave.FormItemStatus = FormItemStatus.Completed;
                            }
                            
                            // Diğer tüm FormItem'ları Completed yap (biri doldurunca diğerleri kapanır)
                            if (formItemToSave != null && item.formItems.Count > 1)
                            {
                                foreach (var formItem in item.formItems)
                                {
                                    if (formItem.Id != formItemToSave.Id)
                                    {
                                        formItem.FormItemStatus = FormItemStatus.Completed;
                                    }
                                }
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

                    Utils utils = new Utils();
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

                // ✅ THREADING FIX: SendMail'i SaveChanges'ten SONRA çağır
                // Navigation property'lere (ApproveUser.Email) erişmeden önce tüm DbContext işlemleri tamamlanmalı
                if (result != null)
                {
                    if (head.workFlowStatus == WorkflowStatus.Completed)
                    {
                        string getUniqApproveId = head.UniqNumber.ToString();
                        SendMail(MailStatus.OnaySureciTamamlandı, head.CreateUser, "", getUniqApproveId);
                    }

                    if (parameters?.UserManager != null)
                    {
                        foreach (var item in head.workflowItems)
                        {
                            if (item.approveItems != null)
                            {
                                foreach (var mail in item.approveItems)
                                {
                                    if (mail.ApproverStatus == ApproverStatus.Pending)
                                    {
                                        // ✅ UserManager ile user'ı bul ve email al (DbContext işlemleri bittikten sonra)
                                        string approveUserEmail = mail.ApproveUserId; // Default: UserId
                                        try
                                        {
                                            var user = await parameters.UserManager.FindByIdAsync(mail.ApproveUserId);
                                            if (user != null)
                                            {
                                                approveUserEmail = user.Email ?? user.UserName ?? mail.ApproveUserId;
                                            }
                                        }
                                        catch
                                        {
                                            // User bulunamazsa ApproveUserId kullan
                                        }
                                        
                                        SendMail(MailStatus.OnayınızaSunuldu, approveUserEmail, mail.ApproveUserNameSurname, head.UniqNumber.ToString(), head.WorkFlowInfo);
                                    }
                                    else if (mail.ApproverStatus == ApproverStatus.Reject)
                                    {
                                        // ✅ UserManager ile runtime user'ı bul ve email al
                                        string runtimeUserEmail = "";
                                        if (!string.IsNullOrEmpty(mail.ApprovedUser_RuntimeId))
                                        {
                                            try
                                            {
                                                var user = await parameters.UserManager.FindByIdAsync(mail.ApprovedUser_RuntimeId);
                                                if (user != null)
                                                {
                                                    runtimeUserEmail = user.Email ?? user.UserName ?? mail.ApprovedUser_RuntimeId;
                                                }
                                            }
                                            catch
                                            {
                                                runtimeUserEmail = mail.ApprovedUser_RuntimeId;
                                            }
                                        }
                                        
                                        SendMail(MailStatus.Reddedildi, runtimeUserEmail, mail.ApproveUserNameSurname, head.UniqNumber.ToString(), head.WorkFlowInfo);
                                    }
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

                // ✅ THREADING FIX: EmployeeAssignments zaten metod başında yüklendi (cache hazır)
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

                // ✅ THREADING FIX: Form bilgilerini AddAsync'den ÖNCE al
                // FormTaskNode için FormDesign'i önceden yükle (DbContext threading sorununu önlemek için)
                FormItems formItemToSave = null;
                FormInstance formInstanceToSave = null;
                
                foreach (var item in head.workflowItems)
                {
                    if (item.NodeType == "formTaskNode" && item.formItems != null && item.formItems.Count > 0)
                    {
                        formItemToSave = item.formItems.FirstOrDefault();
                        if (formItemToSave != null)
                        {
                            // FormDesign'i belirle: FormItem'dan veya FormId varsa Form tablosundan
                            string formDesign = formItemToSave.FormDesign;
                            
                            // Eğer FormDesign boşsa ve FormId varsa, Form tablosundan al
                            // ✅ AddAsync çağrısından ÖNCE yapıyoruz (threading fix)
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
                            
                            // FormData her zaman gelir (formdan butonla action ve form verisi gelir)
                            // FormItem'a da FormData'yı kaydet
                            if (!string.IsNullOrEmpty(payloadJson))
                            {
                                formItemToSave.FormData = payloadJson;
                            }
                            
                            break; // İlk FormTaskNode'u bulduktan sonra çık
                        }
                    }
                }

                // ✅ THREADING FIX: head'i DbContext'e eklemeden önce navigation property'leri temizle
                // Böylece lazy loading tetiklenmez ve threading sorunu oluşmaz
                foreach (var item in head.workflowItems)
                {
                    // Navigation property'leri null yap (EF Core lazy loading'i tetiklemesin)
                    // Bu objeler yeni oluşturulmuş, henüz DbContext'e eklenmemiş
                    // EF Core bunları AddAsync sırasında otomatik olarak ilişkilendirecek
                    if (item.formItems != null)
                    {
                        foreach (var formItem in item.formItems)
                        {
                            formItem.FormUser = null; // Navigation property'yi temizle
                        }
                    }
                    if (item.approveItems != null)
                    {
                        foreach (var approveItem in item.approveItems)
                        {
                            approveItem.ApproveUser = null; // Navigation property'yi temizle
                            approveItem.ApprovedUser_Runtime = null; // Navigation property'yi temizle
                        }
                    }
                }

                // ✅ Artık tüm ön hazırlıklar tamamlandı, workflow'u kaydet
                var result = await parameters.workFlowService.AddAsync(head);

                // FormInstance oluştur (result.Id gerekiyor)
                if (result != null && formItemToSave != null)
                {
                    Utils utils = new Utils();
                    string userNameSurname = utils.GetNameAndSurnameAsync(dto.UserName).ToString();
                    
                    formInstanceToSave = new FormInstance
                    {
                        WorkflowHeadId = result.Id,
                        FormId = formItemToSave.FormId,
                        FormDesign = formItemToSave.FormDesign, // Yukarıda doldurduk
                        FormData = payloadJson,
                        UpdatedBy = dto.UserName,
                        UpdatedByNameSurname = userNameSurname
                    };
                    
                    // FormItems ve FormInstance'ı kaydet
                    await parameters.workFlowService.UpdateWorkFlowAndRelations(result, result.workflowItems, null, formItemToSave, formInstanceToSave);
                }

                // ✅ THREADING FIX: SendMail'i SaveChanges'ten SONRA çağır
                // Navigation property'lere (ApproveUser.Email) erişmeden önce tüm DbContext işlemleri tamamlanmalı
                if (result != null && parameters?.UserManager != null)
                {
                    foreach (var item in head.workflowItems)
                    {
                        if (item.approveItems != null)
                        {
                            foreach (var mail in item.approveItems)
                            {
                                if (mail.ApproverStatus == ApproverStatus.Pending)
                                {
                                    string getUniqApproveId = Utils.ShortenGuid(head.Id);
                                    
                                    // ✅ UserManager ile user'ı bul ve email al (DbContext işlemleri bittikten sonra)
                                    string approveUserEmail = mail.ApproveUserId; // Default: UserId
                                    try
                                    {
                                        var user = await parameters.UserManager.FindByIdAsync(mail.ApproveUserId);
                                        if (user != null)
                                        {
                                            approveUserEmail = user.Email ?? user.UserName ?? mail.ApproveUserId;
                                        }
                                    }
                                    catch
                                    {
                                        // User bulunamazsa ApproveUserId kullan
                                    }
                                    
                                    SendMail(MailStatus.OnayınızaSunuldu, approveUserEmail, mail.ApproveUserNameSurname, head.UniqNumber.ToString(), head.WorkFlowInfo);
                                }
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
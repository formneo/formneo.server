using System;
using System.Collections.Generic;
using formneo.core.Configuration;
using formneo.core.Models;

namespace formneo.core.DTOs
{
    /// <summary>
    /// Kullanıcının task'larını döndüren DTO
    /// </summary>
    public class MyTasksDto
    {
        /// <summary>
        /// FormTask listesi (FormTaskNode için)
        /// </summary>
        public List<FormTaskItemDto> FormTasks { get; set; } = new List<FormTaskItemDto>();

        /// <summary>
        /// UserTask listesi (ApproverNode için)
        /// </summary>
        public List<UserTaskItemDto> UserTasks { get; set; } = new List<UserTaskItemDto>();

        /// <summary>
        /// Toplam pending task sayısı
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// FormTask item DTO (FormTaskNode için)
    /// </summary>
    public class FormTaskItemDto
    {
        public Guid Id { get; set; }
        public Guid WorkflowItemId { get; set; }
        public Guid WorkflowHeadId { get; set; }
        public string ShortId { get; set; }
        public string ShortWorkflowItemId { get; set; }
        
        /// <summary>
        /// FormDesign JSON verisi (form render için)
        /// </summary>
        public string? FormDesign { get; set; }
        
        /// <summary>
        /// Form ID
        /// </summary>
        public Guid? FormId { get; set; }
        
        /// <summary>
        /// FormTaskNode mesajı
        /// </summary>
        public string? FormTaskMessage { get; set; }
        
        /// <summary>
        /// Form açıklaması
        /// </summary>
        public string? FormDescription { get; set; }
        
        /// <summary>
        /// Form durumu
        /// </summary>
        public FormItemStatus FormItemStatus { get; set; }
        
        /// <summary>
        /// Workflow bilgileri
        /// </summary>
        public WorkFlowHeadDto WorkFlowHead { get; set; }
        
        /// <summary>
        /// WorkflowItem bilgileri
        /// </summary>
        public WorkFlowItemDto WorkFlowItem { get; set; }
        
        [GmtPlus3]
        public DateTime CreatedDate { get; set; }
        
        public int UniqNumber { get; set; }
        
        /// <summary>
        /// Süreci başlatan kişinin ad-soyad bilgisi
        /// </summary>
        public string BaslatanAdSoyad { get; set; }
        
        /// <summary>
        /// Süreci başlatan kişinin departman bilgisi
        /// </summary>
        public string BaslatanDepartman { get; set; }
        
        /// <summary>
        /// Süreci başlatan kişinin pozisyon bilgisi
        /// </summary>
        public string BaslatanPozisyon { get; set; }
        
        /// <summary>
        /// Süreç adı
        /// </summary>
        public string SurecAdi { get; set; }
        
        /// <summary>
        /// Form adı
        /// </summary>
        public string FormAdi { get; set; }
        
        /// <summary>
        /// Mevcut adım (pending olan item'ın node name'i)
        /// </summary>
        public string MevcutAdim { get; set; }
        
        /// <summary>
        /// Geçen süre (gün cinsinden)
        /// </summary>
        public int Sure { get; set; }
        
        /// <summary>
        /// Geçen süre (detaylı format: "3 gün önce")
        /// </summary>
        public string SureDetayli { get; set; }
        
        /// <summary>
        /// Durum (Türkçe string: "Devam Ediyor", "Tamamlandı" vb.)
        /// </summary>
        public string Durum { get; set; }
    }

    /// <summary>
    /// UserTask item DTO (ApproverNode için)
    /// </summary>
    public class UserTaskItemDto
    {
        public Guid Id { get; set; }
        public Guid WorkflowItemId { get; set; }
        public Guid WorkflowHeadId { get; set; }
        public string ShortId { get; set; }
        public string ShortWorkflowItemId { get; set; }
        
        public string ApproveUser { get; set; }
        public string ApproveUserNameSurname { get; set; }
        public ApproverStatus ApproverStatus { get; set; }
        
        /// <summary>
        /// Workflow bilgileri
        /// </summary>
        public WorkFlowHeadDto WorkFlowHead { get; set; }
        
        /// <summary>
        /// WorkflowItem bilgileri
        /// </summary>
        public WorkFlowItemDto WorkFlowItem { get; set; }
        
        [GmtPlus3]
        public DateTime CreatedDate { get; set; }
        
        public int UniqNumber { get; set; }
        
        /// <summary>
        /// Süreci başlatan kişinin ad-soyad bilgisi
        /// </summary>
        public string BaslatanAdSoyad { get; set; }
        
        /// <summary>
        /// Süreci başlatan kişinin departman bilgisi
        /// </summary>
        public string BaslatanDepartman { get; set; }
        
        /// <summary>
        /// Süreci başlatan kişinin pozisyon bilgisi
        /// </summary>
        public string BaslatanPozisyon { get; set; }
        
        /// <summary>
        /// Süreç adı
        /// </summary>
        public string SurecAdi { get; set; }
        
        /// <summary>
        /// Form adı
        /// </summary>
        public string FormAdi { get; set; }
        
        /// <summary>
        /// Mevcut adım (pending olan item'ın node name'i)
        /// </summary>
        public string MevcutAdim { get; set; }
        
        /// <summary>
        /// Geçen süre (gün cinsinden)
        /// </summary>
        public int Sure { get; set; }
        
        /// <summary>
        /// Geçen süre (detaylı format: "3 gün önce")
        /// </summary>
        public string SureDetayli { get; set; }
        
        /// <summary>
        /// Durum (Türkçe string: "Devam Ediyor", "Tamamlandı" vb.)
        /// </summary>
        public string Durum { get; set; }
    }

    /// <summary>
    /// Görev detayı için DTO (WorkflowItem tipine göre FormTask veya UserTask bilgilerini döndürmek için)
    /// BMP mantığı: WorkflowItem'ın NodeType'ına göre farklı görev detayları gösterilir
    /// </summary>
    public class TaskFormDto
    {
        /// <summary>
        /// WorkflowItem'ın NodeType'ı (formTaskNode veya approverNode)
        /// </summary>
        public string NodeType { get; set; }

        /// <summary>
        /// WorkflowItem'ın NodeName'i
        /// </summary>
        public string? NodeName { get; set; }

        /// <summary>
        /// Görev tipi ("formTask" veya "userTask")
        /// </summary>
        public string TaskType { get; set; }

        /// <summary>
        /// WorkflowHead ID
        /// </summary>
        public Guid WorkflowHeadId { get; set; }

        /// <summary>
        /// WorkflowItem ID
        /// </summary>
        public Guid WorkflowItemId { get; set; }

        /// <summary>
        /// FormDesign JSON verisi (form tasarımı)
        /// </summary>
        public string? FormDesign { get; set; }

        /// <summary>
        /// Form verileri (JSON) - FormInstance'dan alınan güncel veri
        /// </summary>
        public string? FormData { get; set; }

        /// <summary>
        /// Form ID
        /// </summary>
        public Guid? FormId { get; set; }

        // ========== FormTask (formTaskNode) için bilgiler ==========
        
        /// <summary>
        /// FormTask için: FormItem ID
        /// </summary>
        public Guid? FormItemId { get; set; }

        /// <summary>
        /// FormTask için: FormTaskMessage (görev mesajı)
        /// </summary>
        public string? FormTaskMessage { get; set; }

        /// <summary>
        /// FormTask için: FormDescription (form açıklaması)
        /// </summary>
        public string? FormDescription { get; set; }

        /// <summary>
        /// FormTask için: FormUser (formu dolduran kullanıcı)
        /// </summary>
        public string? FormUser { get; set; }

        /// <summary>
        /// FormTask için: FormItemStatus (Pending, Completed, Rejected)
        /// </summary>
        public FormItemStatus? FormItemStatus { get; set; }

        /// <summary>
        /// FormTask için: FieldScript (workflow definition'dan alınan fieldScript)
        /// </summary>
        public string? FieldScript { get; set; }

        /// <summary>
        /// Node'un script'i (scriptNode için - workflow definition'dan alınan script)
        /// </summary>
        public string? NodeScript { get; set; }

        // ========== UserTask (approverNode) için bilgiler ==========
        
        /// <summary>
        /// UserTask için: ApproveItem ID
        /// </summary>
        public Guid? ApproveItemId { get; set; }

        /// <summary>
        /// UserTask için: ApproveUser (onaylayan kullanıcı)
        /// </summary>
        public string? ApproveUser { get; set; }

        /// <summary>
        /// UserTask için: ApproveUserNameSurname
        /// </summary>
        public string? ApproveUserNameSurname { get; set; }

        /// <summary>
        /// UserTask için: ApproverStatus (Pending, Approve, Reject, Send)
        /// </summary>
        public ApproverStatus? ApproverStatus { get; set; }

        /// <summary>
        /// UserTask için: WorkFlowDescription (onay açıklaması)
        /// </summary>
        public string? WorkFlowDescription { get; set; }
    }
}


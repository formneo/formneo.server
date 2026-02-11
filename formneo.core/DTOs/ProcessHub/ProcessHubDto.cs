using System;
using System.Collections.Generic;

namespace formneo.core.DTOs.ProcessHub
{
    /// <summary>
    /// Process Hub view tipleri
    /// </summary>
    public enum ProcessViewType
    {
        /// <summary>
        /// Kullanıcının kendi oluşturduğu kayıtlar
        /// </summary>
        My,
        
        /// <summary>
        /// Kullanıcının onaylaması gereken kayıtlar
        /// </summary>
        Approvals,
        
        /// <summary>
        /// Tüm kayıtlar (yetkiye göre)
        /// </summary>
        All
    }

    /// <summary>
    /// Process Hub data response
    /// </summary>
    public class ProcessHubDataResponseDto
    {
        public List<ProcessHubItemDto> Data { get; set; } = new List<ProcessHubItemDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Process Hub tek bir item - WorkflowHead bazında (My/All) veya WorkflowItem bazında (Approvals)
    /// </summary>
    public class ProcessHubItemDto
    {
        /// <summary>
        /// WorkflowHead Id - GetDetail/History için
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// WorkflowItem Id - Approvals görünümünde satır bazında (onay/form aksiyonu için)
        /// </summary>
        public Guid? WorkflowItemId { get; set; }
        public Guid WorkflowDefinationId { get; set; }
        public string WorkflowName { get; set; }
        public string CurrentNodeId { get; set; }
        public string CurrentNodeName { get; set; }
        public string WorkFlowStatus { get; set; }
        public string WorkFlowStatusText { get; set; }
        public string WorkFlowInfo { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        /// <summary>
        /// Form instance bilgileri (varsa)
        /// </summary>
        public Guid? FormId { get; set; }
        public string FormName { get; set; }
        public string FormData { get; set; }
        
        /// <summary>
        /// Kullanıcının bu workflow'daki pending işlemleri
        /// </summary>
        public int PendingApprovalCount { get; set; }
        public int PendingFormCount { get; set; }
        
        /// <summary>
        /// Bu workflow'daki pending FormItems'ların FormUserId'leri
        /// (Kullanıcının doldurması gereken formların user ID'leri)
        /// </summary>
        public List<string> FormUserIds { get; set; } = new List<string>();
    }

    /// <summary>
    /// Workflow history response DTO
    /// </summary>
    public class WorkflowHistoryResponseDto
    {
        /// <summary>
        /// Workflow head bilgisi (süreci kim başlattı, ne zaman)
        /// </summary>
        public WorkflowHeadInfoDto HeadInfo { get; set; }
        
        /// <summary>
        /// Workflow item history listesi (kronolojik sırada)
        /// </summary>
        public List<WorkflowItemHistoryDto> HistoryItems { get; set; } = new List<WorkflowItemHistoryDto>();
        
        /// <summary>
        /// Toplam history item sayısı
        /// </summary>
        public int TotalItemCount { get; set; }
    }

    /// <summary>
    /// Workflow head bilgileri (kim başlattı, ne zaman)
    /// </summary>
    public class WorkflowHeadInfoDto
    {
        public Guid Id { get; set; }
        public string WorkflowName { get; set; }
        public Guid WorkflowDefinationId { get; set; }
        
        /// <summary>
        /// Süreci başlatan kullanıcı
        /// </summary>
        public string StartedBy { get; set; }
        
        /// <summary>
        /// Süreç başlangıç tarihi
        /// </summary>
        public DateTime StartedDate { get; set; }
        
        public string CurrentNodeId { get; set; }
        public string CurrentNodeName { get; set; }
        public string WorkFlowStatus { get; set; }
        public string WorkFlowStatusText { get; set; }
        public string WorkFlowInfo { get; set; }
        
        /// <summary>
        /// Form instance bilgileri
        /// </summary>
        public Guid? FormId { get; set; }
        public string FormName { get; set; }
        public string FormData { get; set; }
    }

    /// <summary>
    /// Workflow item history DTO - Basit ve temiz
    /// </summary>
    public class WorkflowItemHistoryDto
    {
        public Guid Id { get; set; }
        public Guid WorkflowHeadId { get; set; }
        public string WorkflowNodeId { get; set; }
        
        /// <summary>
        /// Node bilgileri
        /// </summary>
        public string NodeName { get; set; }
        public string NodeDescription { get; set; }
        public string NodeType { get; set; }
        public string NodeStatus { get; set; }
        public string NodeStatusText { get; set; }
        
        /// <summary>
        /// Duruma göre özet: Beklemede ise "Ahmet Yılmaz, Mehmet Demir", Tamamlandı ise "Mehmet Demir - 10.02.2025"
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// İşlem tarihi (Tamamlandı ise tamamlanma tarihi, Beklemede ise null)
        /// </summary>
        public DateTime? OperationDate { get; set; }
        
        /// <summary>
        /// Tarih bilgileri
        /// </summary>
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

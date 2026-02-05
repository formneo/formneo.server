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
    /// Process Hub tek bir workflow item'ı
    /// </summary>
    public class ProcessHubItemDto
    {
        public Guid Id { get; set; }
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
    }
}

using System;

namespace formneo.core.DTOs
{
    /// <summary>
    /// Workflow filtreleme için DTO (MVP - Basit)
    /// </summary>
    public class WorkflowFilterDto
    {
        /// <summary>
        /// Süreç tipi (WorkFlowDefination ID)
        /// </summary>
        public Guid? WorkFlowDefinationId { get; set; }
        
        /// <summary>
        /// Durum (1=InProgress, 2=Completed, 3=Pending, 5=Draft, 6=Cancelled)
        /// </summary>
        public int? Durum { get; set; }
        
        /// <summary>
        /// Başlangıç tarihi (bu tarihten sonra)
        /// </summary>
        public DateTime? BaslangicTarihiMin { get; set; }
        
        /// <summary>
        /// Bitiş tarihi (bu tarihten önce)
        /// </summary>
        public DateTime? BaslangicTarihiMax { get; set; }
    }
}


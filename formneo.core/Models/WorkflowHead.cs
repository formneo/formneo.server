using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.Models.BudgetManagement;

namespace formneo.core.Models
{

    /// <summary>
    /// WorkflowHead: Taslak, Başladı, Bitti, İptal. Detaylar workflowItems'dan takip edilir.
    /// Pending sadece WorkflowItem için (node bekliyor).
    /// </summary>
    public enum WorkflowStatus
    {
        Draft = 5,      // Taslak
        InProgress = 1, // Başladı
        Completed = 2,   // Bitti
        Cancelled = 6,  // İptal edildi
        Pending = 3,    // WorkflowItem için (WorkflowHead'de kullanılmaz)
    }

    public class WorkflowHead : BaseEntity
    {

         public string? WorkflowName { get; set; }

        public string? CurrentNodeId { get; set; }

        public string? CurrentNodeName { get; set; }

        public WorkflowStatus? workFlowStatus { get; set; }

        public string? WorkFlowInfo { get; set; }
        public string CreateUser { get; set; }

        public virtual List<WorkflowItem>? workflowItems { get; set; }

        /// <summary>
        /// WorkflowHead'e bağlı form instance'ı (son/güncel form verisi)
        /// </summary>
        public virtual FormInstance? FormInstance { get; set; }

        /// <summary>
        /// Ana form ID (tek ana form üzerinden ilerleme için)
        /// </summary>
        [ForeignKey("Form")]
        public Guid? FormId { get; set; }

        /// <summary>
        /// Form ile ilişki
        /// </summary>
        public virtual Form? Form { get; set; }

        [ForeignKey("WorkFlowDefination")]
        public Guid WorkFlowDefinationId { get; set; }

        public virtual WorkFlowDefination WorkFlowDefination { get; set; }

        public string WorkFlowDefinationJson { get; set; }




    }





}

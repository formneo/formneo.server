using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.Configuration;
using formneo.core.Models;

namespace formneo.core.DTOs
{
    public class FormItemsDtoResult
    {
        public int Count { get; set; }

        public List<FormItemsDto> FormItemsDtoList { get; set; }
    }

    public class FormItemsDto
    {
        public Guid Id { get; set; }
        public Guid WorkflowItemId { get; set; }

        public string ShortId { get; set; }
        public string ShortWorkflowItemId { get; set; }

        /// <summary>
        /// FormDesign JSON verisi
        /// </summary>
        public string? FormDesign { get; set; }

        /// <summary>
        /// Form ID
        /// </summary>
        public Guid? FormId { get; set; }

        /// <summary>
        /// Formu dolduran/dolduracak kullanıcı ID (Foreign Key)
        /// </summary>
        public string FormUserId { get; set; }

        /// <summary>
        /// Formu dolduran kullanıcı email/username (display için)
        /// </summary>
        public string? FormUser { get; set; }

        /// <summary>
        /// Formu dolduran kullanıcının adı soyadı
        /// </summary>
        public string? FormUserNameSurname { get; set; }

        /// <summary>
        /// Form verileri (JSON)
        /// </summary>
        public string? FormData { get; set; }

        /// <summary>
        /// Form açıklaması
        /// </summary>
        public string? FormDescription { get; set; }

        /// <summary>
        /// Kullanıcının workflow'da yazdığı mesaj/not
        /// </summary>
        public string? FormUserMessage { get; set; }

        /// <summary>
        /// FormTaskNode oluşturulurken kaydedilen mesaj (component'te gösterilecek)
        /// </summary>
        public string? FormTaskMessage { get; set; }

        /// <summary>
        /// Form durumu
        /// </summary>
        public FormItemStatus FormItemStatus { get; set; }

        public WorkFlowItemDto workFlowItem { get; set; }

        [GmtPlus3]
        public DateTime CreatedDate { get; set; }

        [GmtPlus3]
        public DateTime UpdatedDate { get; set; }

        public int UniqNumber { get; set; }

        public WorkFlowHeadDto WorkFlowHead { get; set; }
    }

    public class FormHeadInfo
    {
        public int PendingCount { get; set; }

        public int CompletedCount { get; set; }

        public int RejectedCount { get; set; }

        public List<FormItemsDto> items { get; set; }
    }
}


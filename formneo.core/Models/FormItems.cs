using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace formneo.core.Models
{
    public enum FormItemStatus
    {
        Pending,
        Completed,
        Rejected
    }

    public class FormItems : BaseEntity
    {
        [ForeignKey("WorkFlowItem")]
        public Guid WorkflowItemId { get; set; }

        /// <summary>
        /// FormDesign JSON verisi (form tasarımı)
        /// </summary>
        public string? FormDesign { get; set; }

        /// <summary>
        /// Form ID (Form tablosuna referans)
        /// </summary>
        [ForeignKey("Form")]
        public Guid? FormId { get; set; }

        /// <summary>
        /// Formu dolduran/dolduracak kullanıcı ID (Foreign Key - AspNetUsers.Id)
        /// </summary>
        [ForeignKey("FormUser")]
        public string FormUserId { get; set; }

        /// <summary>
        /// Kullanıcı navigation property
        /// </summary>
        public virtual UserApp FormUser { get; set; }

        /// <summary>
        /// Formu dolduran kullanıcının adı soyadı (Snapshot - değişmez, tarihsel kayıt)
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

        /// <summary>
        /// WorkflowItem ile ilişki
        /// </summary>
        public virtual WorkflowItem WorkflowItem { get; set; }

        /// <summary>
        /// Form ile ilişki
        /// </summary>
        public virtual Form? Form { get; set; }
    }
}


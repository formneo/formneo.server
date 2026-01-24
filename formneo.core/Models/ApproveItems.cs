using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace formneo.core.Models
{

    public enum ApproverStatus
    {
        Pending,
        Approve,
        Reject,
        Send
    }

    public class ApproveItems : BaseEntity
    {
        [ForeignKey("WorkFlowItem")]
        public Guid WorkflowItemId { get; set; }

        /// <summary>
        /// Onaylayacak kullanıcı ID (Foreign Key - AspNetUsers.Id)
        /// </summary>
        [ForeignKey("ApproveUser")]
        public string ApproveUserId { get; set; }

        /// <summary>
        /// Kullanıcı navigation property
        /// </summary>
        public virtual UserApp ApproveUser { get; set; }

        /// <summary>
        /// Onaylayacak kullanıcının adı soyadı (Snapshot - değişmez)
        /// </summary>
        public string? ApproveUserNameSurname { get; set; }

        /// <summary>
        /// Runtime'da onaylayan kullanıcı ID (Foreign Key)
        /// </summary>
        [ForeignKey("ApprovedUser_Runtime")]
        public string? ApprovedUser_RuntimeId { get; set; }

        /// <summary>
        /// Runtime kullanıcı navigation property
        /// </summary>
        public virtual UserApp? ApprovedUser_Runtime { get; set; }

        /// <summary>
        /// Runtime kullanıcının adı soyadı (Snapshot)
        /// </summary>
        public string? ApprovedUser_RuntimeNameSurname { get; set; }

        public string? ApprovedUser_RuntimeNote { get; set; }
        public string? ApprovedUser_RuntimeNumberManDay { get; set; }

        public string WorkFlowDescription { get; set; }

        public ApproverStatus ApproverStatus { get; set; }
        public virtual WorkflowItem WorkflowItem { get; set; }
    }
}

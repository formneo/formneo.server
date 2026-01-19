using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace formneo.core.Models
{
    public class UserTenant : GlobalBaseEntity
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public virtual UserApp User { get; set; }

        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        public virtual MainClient Tenant { get; set; }

        public bool IsActive { get; set; }

        // Tenant-scoped user permissions and preferences
        public bool HasTicketPermission { get; set; }
        public bool HasDepartmentPermission { get; set; }
        public bool HasOtherCompanyPermission { get; set; }
        public bool HasOtherDeptCalendarPerm { get; set; }

        public bool canEditTicket { get; set; }
        public bool DontApplyDefaultFilters { get; set; }

        public string? PCname { get; set; }

        // Reset şifre alanları tenant-bazlı değildir; UserApp üzerinde tutulur
        // Manager bilgisi UserApp.ManagerId'den alınır (Single Source of Truth)
    }
}



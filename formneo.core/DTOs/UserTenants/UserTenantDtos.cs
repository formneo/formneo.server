using System;

namespace formneo.core.DTOs.UserTenants
{
    public class UserTenantFullDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string TenantName { get; set; }
        public string TenantSlug { get; set; }
    }

    public class UserTenantByTenantDto
    {
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string TenantName { get; set; }
    }

    public class UserTenantListDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Tenant scoped fields
        public bool HasTicketPermission { get; set; }
        public bool HasDepartmentPermission { get; set; }
        public bool HasOtherCompanyPermission { get; set; }
        public bool HasOtherDeptCalendarPerm { get; set; }

        public bool canEditTicket { get; set; }
        public bool DontApplyDefaultFilters { get; set; }

        public string? PCname { get; set; }

        // Reset şifre alanları tenant-bazlı değildir
        // Manager bilgisi UserApp.ManagerId'den alınır (Single Source of Truth)
    }

    public class UserTenantInsertDto
    {
        public string UserId { get; set; }
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; } = true;

        // Tenant scoped fields
        public bool HasTicketPermission { get; set; }
        public bool HasDepartmentPermission { get; set; }
        public bool HasOtherCompanyPermission { get; set; }
        public bool HasOtherDeptCalendarPerm { get; set; }

        public bool canEditTicket { get; set; }
        public bool DontApplyDefaultFilters { get; set; }

        public string? PCname { get; set; }

        // Reset şifre alanları tenant-bazlı değildir
        // Manager bilgisi UserApp.ManagerId'den alınır (Single Source of Truth)
    }

    public class UserTenantUpdateDto
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }

        // Tenant scoped fields
        public bool HasTicketPermission { get; set; }
        public bool HasDepartmentPermission { get; set; }
        public bool HasOtherCompanyPermission { get; set; }
        public bool HasOtherDeptCalendarPerm { get; set; }

        public bool canEditTicket { get; set; }
        public bool DontApplyDefaultFilters { get; set; }

        public string? PCname { get; set; }

        // Reset şifre alanları tenant-bazlı değildir
        // Manager bilgisi UserApp.ManagerId'den alınır (Single Source of Truth)
    }

    // Kaldırıldı: sadece tek tenant-çoklu user senaryosu desteklenecek

    // Tek tenant için birden fazla kullanıcı atama (temizle + ekle)
    public class UserTenantBulkAssignUsersDto
    {
        public Guid TenantId { get; set; }
        public List<string> UserIds { get; set; } = new List<string>();
        public bool IsActive { get; set; } = true;
    }

    public class UserTenantWithAdminFlagDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Tenant information
        public string TenantName { get; set; }
        public string TenantSlug { get; set; }

        // Tenant scoped fields
        public bool HasTicketPermission { get; set; }
        public bool HasDepartmentPermission { get; set; }
        public bool HasOtherCompanyPermission { get; set; }
        public bool HasOtherDeptCalendarPerm { get; set; }

        public bool canEditTicket { get; set; }
        public bool DontApplyDefaultFilters { get; set; }

        public string? PCname { get; set; }

        // Manager bilgisi UserApp.ManagerId'den alınır (Single Source of Truth)
        // Admin flag
        public bool IsTenantAdmin { get; set; }

        public static UserTenantWithAdminFlagDto From(object source, bool isTenantAdmin)
        {
            var dto = new UserTenantWithAdminFlagDto
            {
                IsTenantAdmin = isTenantAdmin
            };

            // Use reflection to copy properties from source object
            var sourceType = source.GetType();
            var targetType = typeof(UserTenantWithAdminFlagDto);

            foreach (var sourceProp in sourceType.GetProperties())
            {
                var targetProp = targetType.GetProperty(sourceProp.Name);
                if (targetProp != null && targetProp.CanWrite && sourceProp.CanRead)
                {
                    var value = sourceProp.GetValue(source);
                    targetProp.SetValue(dto, value);
                }
            }

            return dto;
        }
    }
}



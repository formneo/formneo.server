using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.Models;

namespace formneo.core.DTOs
{
    public class CreateUserDto
    {
        public string? Company { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? profileInfo { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isSystemAdmin { get; set; }
        public bool isBlocked { get; set; }
        public bool? isTestData { get; set; }
        public bool vacationMode { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? LastLoginIp { get; set; }
        public bool canSsoLogin { get; set; }
        public string? photo { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        // tenant-bazlı alanlar UserTenant DTO'larına taşındı
        public string? Title { get; set; }
        public string? LinkedinUrl { get; set; }
        public Guid? OrgUnitId { get; set; }
        // Manager bilgisi OrgUnit.ManagerId'den alınır: user.OrgUnit?.Manager
        // Kullanıcı oluşturulurken atanacak roller
        public List<UserRoleDto> RoleIds { get; set; }

        // tenant-bazlı alanlar UserTenant DTO’larına taşındı

        public Guid? PositionId { get; set; }
        public UserLevel? UserLevel { get; set; }
        // tenant-bazlı alanlar UserTenant DTO’larına taşındı
    }
    public class UpdateUserDto
    {
        public string Id { get; set; }
        public string? Company { get; set; }
        public string? profileInfo { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isSystemAdmin { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? LastLoginIp { get; set; }
        public bool canSsoLogin { get; set; }
        public bool isBlocked { get; set; }
        public bool? isTestData { get; set; }
        public bool vacationMode { get; set; }
        public string? photo { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        // tenant-bazlı alanlar UserTenant DTO'larına taşındı
        public string? Title { get; set; }
        public string? LinkedinUrl { get; set; }
        public Guid? OrgUnitId { get; set; }
        // Manager bilgisi OrgUnit.ManagerId'den alınır: user.OrgUnit?.Manager
        // Kullanıcı güncellenirken atanacak roller
        public List<UserRoleDto> RoleIds { get; set; }

        // tenant-bazlı alanlar UserTenant DTO’larına taşındı
        public Guid? PositionId { get; set; }
        public UserLevel? UserLevel { get; set; }
        public string? ResetPasswordCode { get; set; }
        public DateTime? ResetCodeExpiry { get; set; }
        // tenant-bazlı alanlar UserTenant DTO’larına taşındı
    }
}

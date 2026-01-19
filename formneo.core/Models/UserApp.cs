using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.Models.Ticket;

namespace formneo.core.Models
{
    public class UserApp : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public bool isSystemAdmin { get; set; }
        public bool canSsoLogin { get; set; }
        public bool isBlocked { get; set; }
        public bool? isTestData { get; set; }
        public bool vacationMode { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? LastLoginIp { get; set; }

        public string? profileInfo { get; set; }
        public string? Title { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? photo { get; set; }

        // NOT: OrgUnitId, PositionId ve ManagerId artık EmployeeAssignment tablosunda tutuluyor
        // Aktif atama için: EmployeeAssignment tablosundan alınmalı

        // tenant-bazlı alanlar UserTenant'a taşındı
        public string? ResetPasswordCode { get; set; }
        public DateTime? ResetCodeExpiry { get; set; }
        // tenant-bazlı alanlar UserTenant'a taşındı
        public virtual List<DepartmentUser> DepartmentUsers { get; set; } = new List<DepartmentUser>();

        /// <summary>
        /// Kullanıcının tüm atamaları (geçmiş ve aktif)
        /// </summary>
        public virtual List<EmployeeAssignment> EmployeeAssignments { get; set; } = new List<EmployeeAssignment>();

        public UserLevel UserLevel { get; set; }



    }
    public enum UserLevel
    {
        [Description("Junior")]
        junior = 1,
        [Description("Mid")]
        mid = 2,
        [Description("Senior")]
        senior = 3,
    }

}

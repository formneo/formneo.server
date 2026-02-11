using System;

namespace formneo.core.DTOs
{
    /// <summary>
    /// Kullanıcının organizasyon bilgileri (Ad Soyad, Departman, Pozisyon)
    /// Tüm projede kullanılabilecek ortak DTO
    /// </summary>
    public class UserOrgInfoDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Department { get; set; }
        public Guid? DepartmentId { get; set; }
        public string Position { get; set; }
        public Guid? PositionId { get; set; }
    }
}

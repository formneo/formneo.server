using System.Collections.Generic;
using System.Threading.Tasks;
using formneo.core.DTOs;

namespace formneo.core.Services
{
    /// <summary>
    /// Kullanıcı Ad Soyad, Departman ve Pozisyon bilgilerini sağlayan servis.
    /// Tüm projede ortak kullanım - ProcessHub, Workflow, Ticket vb.
    /// </summary>
    public interface IUserOrgInfoService
    {
        /// <summary>
        /// Birden fazla kullanıcı için org bilgilerini tek sorguda getirir (yüksek performans)
        /// </summary>
        /// <param name="userIds">Kullanıcı ID listesi</param>
        /// <returns>UserId -> UserOrgInfoDto dictionary</returns>
        Task<Dictionary<string, UserOrgInfoDto>> GetUserOrgInfosBatchAsync(IEnumerable<string> userIds);

        /// <summary>
        /// Tek kullanıcı için org bilgisi getirir
        /// </summary>
        Task<UserOrgInfoDto> GetUserOrgInfoAsync(string userId);
    }
}

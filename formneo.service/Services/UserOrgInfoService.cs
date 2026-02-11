using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using formneo.core.DTOs;
using formneo.core.Models;
using formneo.core.Repositories;
using formneo.core.Services;

namespace formneo.service.Services
{
    /// <summary>
    /// Kullanıcı Ad Soyad, Departman ve Pozisyon bilgilerini sağlayan servis.
    /// EmployeeAssignment (aktif atama) + UserApp + OrgUnit + Positions üzerinden tek sorguda batch lookup.
    /// </summary>
    public class UserOrgInfoService : IUserOrgInfoService
    {
        private readonly IGenericRepository<EmployeeAssignment> _employeeAssignmentRepo;
        private readonly IGenericRepository<UserApp> _userRepo;

        public UserOrgInfoService(
            IGenericRepository<EmployeeAssignment> employeeAssignmentRepo,
            IGenericRepository<UserApp> userRepo)
        {
            _employeeAssignmentRepo = employeeAssignmentRepo;
            _userRepo = userRepo;
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, UserOrgInfoDto>> GetUserOrgInfosBatchAsync(IEnumerable<string> userIds)
        {
            var userIdList = userIds?.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList() ?? new List<string>();
            if (userIdList.Count == 0)
                return new Dictionary<string, UserOrgInfoDto>();

            var now = DateTime.UtcNow;

            // Tek sorguda: Aktif atamalar + User + OrgUnit + Position (Include IQueryable extension)
            var data = await _employeeAssignmentRepo
                .Where(ea => userIdList.Contains(ea.UserId) && (ea.EndDate == null || ea.EndDate > now))
                .Include(ea => ea.User)
                .Include(ea => ea.OrgUnit)
                .Include(ea => ea.Position)
                .Select(ea => new
                {
                    ea.UserId,
                    UserName = ea.User != null ? (ea.User.FirstName + " " + ea.User.LastName).Trim() : (string)null,
                    Department = ea.OrgUnit != null ? ea.OrgUnit.Name : (string)null,
                    ea.OrgUnitId,
                    Position = ea.Position != null ? ea.Position.Name : (string)null,
                    ea.PositionId
                })
                .ToListAsync();

            var result = new Dictionary<string, UserOrgInfoDto>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in data)
            {
                if (!result.ContainsKey(item.UserId))
                {
                    result[item.UserId] = new UserOrgInfoDto
                    {
                        UserId = item.UserId,
                        UserName = item.UserName ?? item.UserId,
                        Department = item.Department,
                        DepartmentId = item.OrgUnitId,
                        Position = item.Position,
                        PositionId = item.PositionId
                    };
                }
            }

            // Ataması olmayan kullanıcılar için UserApp'tan Ad Soyad al
            var missingUserIds = userIdList.Where(uid => !result.ContainsKey(uid)).ToList();
            if (missingUserIds.Count > 0)
            {
                var users = await _userRepo
                    .Where(u => missingUserIds.Contains(u.Id))
                    .Select(u => new { u.Id, UserName = (u.FirstName + " " + u.LastName).Trim() })
                    .ToListAsync();

                foreach (var u in users)
                {
                    result[u.Id] = new UserOrgInfoDto
                    {
                        UserId = u.Id,
                        UserName = !string.IsNullOrWhiteSpace(u.UserName) ? u.UserName : u.Id,
                        Department = null,
                        DepartmentId = null,
                        Position = null,
                        PositionId = null
                    };
                }

                foreach (var uid in missingUserIds.Where(uid => !result.ContainsKey(uid)))
                {
                    result[uid] = new UserOrgInfoDto
                    {
                        UserId = uid,
                        UserName = uid,
                        Department = null,
                        DepartmentId = null,
                        Position = null,
                        PositionId = null
                    };
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<UserOrgInfoDto> GetUserOrgInfoAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var dict = await GetUserOrgInfosBatchAsync(new[] { userId });
            return dict.GetValueOrDefault(userId);
        }
    }
}

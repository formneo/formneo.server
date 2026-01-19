using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using formneo.core.Models;

namespace formneo.core.Helpers
{
    /// <summary>
    /// EmployeeAssignment için helper metodlar
    /// NOT: EmployeeAssignment tenant-bağımlıdır (MainClientId)
    /// Her tenant'ta farklı organizasyon yapısı ve atamalar olabilir
    /// </summary>
    public static class EmployeeAssignmentHelper
    {
        /// <summary>
        /// Kullanıcının aktif atamasını bulur (Primary assignment)
        /// Tenant filtresi otomatik olarak AppDbContext query filter'ından gelir
        /// </summary>
        public static async Task<EmployeeAssignment?> GetActiveAssignmentAsync(
            IQueryable<EmployeeAssignment> assignments, 
            string userId)
        {
            var now = DateTime.UtcNow;
            return await assignments
                .Where(ea => ea.UserId == userId 
                    && ea.AssignmentType == AssignmentType.Primary
                    && ea.StartDate <= now
                    && (ea.EndDate == null || ea.EndDate > now))
                .OrderByDescending(ea => ea.StartDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Belirli bir tenant için kullanıcının aktif atamasını bulur
        /// </summary>
        public static async Task<EmployeeAssignment?> GetActiveAssignmentByTenantAsync(
            IQueryable<EmployeeAssignment> assignments,
            string userId,
            Guid tenantId)
        {
            var now = DateTime.UtcNow;
            return await assignments
                .Where(ea => ea.UserId == userId
                    && ea.MainClientId == tenantId
                    && ea.AssignmentType == AssignmentType.Primary
                    && ea.StartDate <= now
                    && (ea.EndDate == null || ea.EndDate > now))
                .OrderByDescending(ea => ea.StartDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Belirli bir tarihteki aktif atamayı bulur
        /// Tenant filtresi otomatik olarak AppDbContext query filter'ından gelir
        /// </summary>
        public static async Task<EmployeeAssignment?> GetAssignmentAtDateAsync(
            IQueryable<EmployeeAssignment> assignments,
            string userId,
            DateTime date)
        {
            return await assignments
                .Where(ea => ea.UserId == userId
                    && ea.AssignmentType == AssignmentType.Primary
                    && ea.StartDate <= date
                    && (ea.EndDate == null || ea.EndDate > date))
                .OrderByDescending(ea => ea.StartDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Belirli bir tenant ve tarihteki aktif atamayı bulur
        /// </summary>
        public static async Task<EmployeeAssignment?> GetAssignmentAtDateByTenantAsync(
            IQueryable<EmployeeAssignment> assignments,
            string userId,
            Guid tenantId,
            DateTime date)
        {
            return await assignments
                .Where(ea => ea.UserId == userId
                    && ea.MainClientId == tenantId
                    && ea.AssignmentType == AssignmentType.Primary
                    && ea.StartDate <= date
                    && (ea.EndDate == null || ea.EndDate > date))
                .OrderByDescending(ea => ea.StartDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Yeni atama oluştururken eski aktif atamayı sonlandırır
        /// Tenant filtresi otomatik olarak AppDbContext query filter'ından gelir
        /// </summary>
        public static async Task EndActiveAssignmentAsync(
            IQueryable<EmployeeAssignment> assignments,
            string userId,
            DateTime endDate)
        {
            var activeAssignment = await GetActiveAssignmentAsync(assignments, userId);
            if (activeAssignment != null && activeAssignment.EndDate == null)
            {
                activeAssignment.EndDate = endDate;
            }
        }

        /// <summary>
        /// Belirli bir tenant için yeni atama oluştururken eski aktif atamayı sonlandırır
        /// </summary>
        public static async Task EndActiveAssignmentByTenantAsync(
            IQueryable<EmployeeAssignment> assignments,
            string userId,
            Guid tenantId,
            DateTime endDate)
        {
            var activeAssignment = await GetActiveAssignmentByTenantAsync(assignments, userId, tenantId);
            if (activeAssignment != null && activeAssignment.EndDate == null)
            {
                activeAssignment.EndDate = endDate;
            }
        }

        /// <summary>
        /// Kullanıcının tüm aktif atamalarını bulur (Primary + Secondary)
        /// Tenant filtresi otomatik olarak AppDbContext query filter'ından gelir
        /// </summary>
        public static IQueryable<EmployeeAssignment> GetActiveAssignments(
            IQueryable<EmployeeAssignment> assignments,
            string userId)
        {
            var now = DateTime.UtcNow;
            return assignments
                .Where(ea => ea.UserId == userId
                    && ea.StartDate <= now
                    && (ea.EndDate == null || ea.EndDate > now));
        }

        /// <summary>
        /// Belirli bir tenant için kullanıcının tüm aktif atamalarını bulur
        /// </summary>
        public static IQueryable<EmployeeAssignment> GetActiveAssignmentsByTenant(
            IQueryable<EmployeeAssignment> assignments,
            string userId,
            Guid tenantId)
        {
            var now = DateTime.UtcNow;
            return assignments
                .Where(ea => ea.UserId == userId
                    && ea.MainClientId == tenantId
                    && ea.StartDate <= now
                    && (ea.EndDate == null || ea.EndDate > now));
        }
    }
}


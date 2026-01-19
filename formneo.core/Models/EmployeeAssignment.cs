using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace formneo.core.Models
{
    /// <summary>
    /// Kullanıcının organizasyon birimi, pozisyon ve yönetici atamalarını tutar.
    /// Geçmiş atamaları saklamak için Effective Dating Pattern kullanır.
    /// 
    /// ÖNEMLİ: Bu model tenant-bağımlıdır (BaseEntity.MainClientId)
    /// Bir kullanıcı farklı tenant'larda farklı organizasyon yapılarında olabilir:
    /// - Tenant A'da IT Departmanında çalışabilir
    /// - Tenant B'de Sales Departmanında çalışabilir
    /// </summary>
    public class EmployeeAssignment : BaseEntity
    {
        /// <summary>
        /// Atanan kullanıcı
        /// </summary>
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public virtual UserApp User { get; set; }

        /// <summary>
        /// Organizasyon birimi (Departman, Takım, vb.)
        /// </summary>
        [ForeignKey(nameof(OrgUnit))]
        public Guid? OrgUnitId { get; set; }
        public virtual OrgUnit? OrgUnit { get; set; }

        /// <summary>
        /// Pozisyon
        /// </summary>
        [ForeignKey(nameof(Position))]
        public Guid? PositionId { get; set; }
        public virtual Positions? Position { get; set; }

        /// <summary>
        /// Yönetici (Direkt rapor edilen kişi)
        /// Bu alan OrgUnit.ManagerId'den farklı olabilir (matrix organizasyon)
        /// </summary>
        [ForeignKey(nameof(Manager))]
        public string? ManagerId { get; set; }
        public virtual UserApp? Manager { get; set; }

        /// <summary>
        /// Atama başlangıç tarihi
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Atama bitiş tarihi (null ise aktif atama)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Bu atama aktif mi?
        /// </summary>
        public bool IsActive => EndDate == null || EndDate > DateTime.UtcNow;

        /// <summary>
        /// Atama tipi (Primary, Secondary, Temporary, Matrix)
        /// </summary>
        public AssignmentType AssignmentType { get; set; } = AssignmentType.Primary;

        /// <summary>
        /// Açıklama/Not
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Atama tipi
    /// </summary>
    public enum AssignmentType
    {
        /// <summary>
        /// Ana atama (asıl departmanı/pozisyonu)
        /// </summary>
        Primary = 1,

        /// <summary>
        /// İkincil atama (proje ekibi, geçici görev)
        /// </summary>
        Secondary = 2,

        /// <summary>
        /// Geçici atama
        /// </summary>
        Temporary = 3,

        /// <summary>
        /// Matrix organizasyon (farklı departmandan yönetici)
        /// </summary>
        Matrix = 4
    }
}


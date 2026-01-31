using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace formneo.core.Models.EntityManager
{
    /// <summary>
    /// Form ile ilişkilendirilebilen entity tanımları (örn: Customer, Order, Employee)
    /// </summary>
    public class FormEntity : BaseEntity
    {
        /// <summary>
        /// Entity adı (örn: Customer, Order, Employee)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string EntityName { get; set; }

        /// <summary>
        /// Entity açıklaması
        /// </summary>
        [MaxLength(500)]
        public string? EntityDescription { get; set; }

        /// <summary>
        /// Entity'nin veritabanındaki tablo adı
        /// </summary>
        [MaxLength(200)]
        public string? TableName { get; set; }

        /// <summary>
        /// Entity'nin schema adı (örn: dbo, custom)
        /// </summary>
        [MaxLength(100)]
        public string? SchemaName { get; set; }

        /// <summary>
        /// Entity'nin namespace'i (örn: formneo.core.Models)
        /// </summary>
        [MaxLength(500)]
        public string? NamespacePath { get; set; }

        /// <summary>
        /// Entity'nin C# sınıf adı
        /// </summary>
        [MaxLength(200)]
        public string? ClassName { get; set; }

        /// <summary>
        /// Entity aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Entity'nin CRUD operasyonları için izinleri
        /// </summary>
        public bool AllowCreate { get; set; } = true;
        public bool AllowRead { get; set; } = true;
        public bool AllowUpdate { get; set; } = true;
        public bool AllowDelete { get; set; } = false;

        /// <summary>
        /// Entity'nin API endpoint'i (örn: /api/customers)
        /// </summary>
        [MaxLength(500)]
        public string? ApiEndpoint { get; set; }

        /// <summary>
        /// Entity'nin display için kullanılacak alan adı (örn: FullName, CompanyName)
        /// </summary>
        [MaxLength(100)]
        public string? DisplayField { get; set; }

        /// <summary>
        /// Entity'nin sıralama için kullanılacak alan adı
        /// </summary>
        [MaxLength(100)]
        public string? OrderByField { get; set; }

        /// <summary>
        /// Bu entity'ye ait alanlar
        /// </summary>
        public virtual ICollection<FormEntityField> Fields { get; set; } = new List<FormEntityField>();

        /// <summary>
        /// Bu entity'yi kullanan form-entity ilişkileri
        /// </summary>
        public virtual ICollection<FormEntityRelation> FormEntityRelations { get; set; } = new List<FormEntityRelation>();

        /// <summary>
        /// Parent entity ile ilişki (self-referencing)
        /// </summary>
        [ForeignKey("ParentEntity")]
        public Guid? ParentEntityId { get; set; }
        public virtual FormEntity? ParentEntity { get; set; }

        /// <summary>
        /// Child entity'ler
        /// </summary>
        public virtual ICollection<FormEntity> ChildEntities { get; set; } = new List<FormEntity>();
    }
}

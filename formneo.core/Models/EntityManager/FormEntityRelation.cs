using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace formneo.core.Models.EntityManager
{
    /// <summary>
    /// Form ile Entity arasındaki ilişkiyi tanımlar
    /// Bir form birden fazla entity'ye bağlanabilir (örn: Customer + Address + Order)
    /// </summary>
    public class FormEntityRelation : BaseEntity
    {
        /// <summary>
        /// Form ID
        /// </summary>
        [Required]
        [ForeignKey("Form")]
        public Guid FormId { get; set; }
        public virtual Form Form { get; set; }

        /// <summary>
        /// Entity ID
        /// </summary>
        [Required]
        [ForeignKey("FormEntity")]
        public Guid FormEntityId { get; set; }
        public virtual FormEntity FormEntity { get; set; }

        /// <summary>
        /// İlişki adı (örn: MainCustomer, BillingAddress, ShippingAddress)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string RelationName { get; set; }

        /// <summary>
        /// İlişki açıklaması
        /// </summary>
        [MaxLength(500)]
        public string? RelationDescription { get; set; }

        /// <summary>
        /// İlişki tipi (OneToOne, OneToMany, ManyToMany)
        /// </summary>
        [Required]
        public EntityRelationType RelationType { get; set; }

        /// <summary>
        /// Bu entity primary mi? (Form'un ana entity'si)
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        /// <summary>
        /// İlişki zorunlu mu?
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// İlişki aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Cascade delete aktif mi?
        /// </summary>
        public bool CascadeDelete { get; set; } = false;

        /// <summary>
        /// İlişki sırası
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Parent relation (nested entities için)
        /// </summary>
        [ForeignKey("ParentRelation")]
        public Guid? ParentRelationId { get; set; }
        public virtual FormEntityRelation? ParentRelation { get; set; }

        /// <summary>
        /// Form data'daki path (JSON path, örn: customer.address.street)
        /// </summary>
        [MaxLength(500)]
        public string? FormDataPath { get; set; }
    }

    /// <summary>
    /// Entity ilişki tipleri
    /// </summary>
    public enum EntityRelationType
    {
        /// <summary>
        /// Bire-bir ilişki
        /// </summary>
        OneToOne = 0,

        /// <summary>
        /// Bire-çok ilişki
        /// </summary>
        OneToMany = 1,

        /// <summary>
        /// Çoka-çok ilişki
        /// </summary>
        ManyToMany = 2,

        /// <summary>
        /// Embedded (gömülü) - ayrı tablo değil, JSON içinde
        /// </summary>
        Embedded = 3
    }
}

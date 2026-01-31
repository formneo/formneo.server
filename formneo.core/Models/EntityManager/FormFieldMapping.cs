using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace formneo.core.Models.EntityManager
{
    /// <summary>
    /// Form elemanları ile entity alanları arasındaki mapping'i tanımlar
    /// Form Design'daki her input, bir entity field'a map edilir
    /// </summary>
    public class FormFieldMapping : BaseEntity
    {
        /// <summary>
        /// Form ID
        /// </summary>
        [Required]
        [ForeignKey("Form")]
        public Guid FormId { get; set; }
        public virtual Form Form { get; set; }

        /// <summary>
        /// Entity Field ID
        /// </summary>
        [Required]
        [ForeignKey("FormEntityField")]
        public Guid FormEntityFieldId { get; set; }
        public virtual FormEntityField FormEntityField { get; set; }

        /// <summary>
        /// Form-Entity ilişkisi (opsiyonel, hangi entity context'inde olduğunu belirtir)
        /// </summary>
        [ForeignKey("FormEntityRelation")]
        public Guid? FormEntityRelationId { get; set; }
        public virtual FormEntityRelation? FormEntityRelation { get; set; }

        /// <summary>
        /// Form design'daki element ID (x-designable-id)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string FormElementId { get; set; }

        /// <summary>
        /// Form design'daki field name (form schema'daki key)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string FormFieldName { get; set; }

        /// <summary>
        /// Form element'in component tipi (Input, Select, DatePicker, etc.)
        /// </summary>
        [MaxLength(200)]
        public string? FormComponentType { get; set; }

        /// <summary>
        /// Mapping aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Read-only mi? (sadece gösterim için, update edilmez)
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        /// <summary>
        /// Auto-mapping mi? (sistem tarafından otomatik oluşturuldu)
        /// </summary>
        public bool IsAutoMapped { get; set; } = false;

        /// <summary>
        /// Transform kuralları (JSON) - veri dönüşüm kuralları
        /// Örn: { "type": "uppercase", "trim": true, "format": "DD/MM/YYYY" }
        /// </summary>
        public string? TransformRules { get; set; }

        /// <summary>
        /// Validation override (JSON) - entity field validation'ı override eder
        /// </summary>
        public string? ValidationOverride { get; set; }

        /// <summary>
        /// Mapping sırası
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Mapping açıklaması / notlar
        /// </summary>
        [MaxLength(500)]
        public string? MappingNotes { get; set; }
    }
}

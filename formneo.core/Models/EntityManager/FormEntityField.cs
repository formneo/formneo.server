using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace formneo.core.Models.EntityManager
{
    /// <summary>
    /// Entity'nin alanlarını tanımlar (örn: Name, Email, Age)
    /// </summary>
    public class FormEntityField : BaseEntity
    {
        /// <summary>
        /// Bağlı olduğu entity
        /// </summary>
        [Required]
        [ForeignKey("FormEntity")]
        public Guid FormEntityId { get; set; }
        public virtual FormEntity FormEntity { get; set; }

        /// <summary>
        /// Alan adı (örn: FirstName, Email, BirthDate)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string FieldName { get; set; }

        /// <summary>
        /// Alan açıklaması
        /// </summary>
        [MaxLength(500)]
        public string? FieldDescription { get; set; }

        /// <summary>
        /// Alan tipi
        /// </summary>
        [Required]
        [ForeignKey("FieldType")]
        public Guid FieldTypeId { get; set; }
        public virtual FormEntityFieldType FieldType { get; set; }

        /// <summary>
        /// Alanın veritabanındaki kolon adı
        /// </summary>
        [MaxLength(200)]
        public string? ColumnName { get; set; }

        /// <summary>
        /// Alanın C# property adı
        /// </summary>
        [MaxLength(200)]
        public string? PropertyName { get; set; }

        /// <summary>
        /// Alan zorunlu mu?
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// Alan unique mi?
        /// </summary>
        public bool IsUnique { get; set; } = false;

        /// <summary>
        /// Alan indexli mi?
        /// </summary>
        public bool IsIndexed { get; set; } = false;

        /// <summary>
        /// Alan nullable mi?
        /// </summary>
        public bool IsNullable { get; set; } = true;

        /// <summary>
        /// Alan aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Alanın maksimum uzunluğu (string için)
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Alanın minimum uzunluğu (string için)
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Alanın minimum değeri (number için)
        /// </summary>
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Alanın maksimum değeri (number için)
        /// </summary>
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Alanın default değeri (JSON formatında)
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Alanın regex pattern'i (validation için)
        /// </summary>
        [MaxLength(500)]
        public string? RegexPattern { get; set; }

        /// <summary>
        /// Regex hata mesajı
        /// </summary>
        [MaxLength(500)]
        public string? RegexErrorMessage { get; set; }

        /// <summary>
        /// Alan sırası (display order)
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Form üzerinde görüntülenecek label
        /// </summary>
        [MaxLength(200)]
        public string? DisplayLabel { get; set; }

        /// <summary>
        /// Form üzerinde placeholder
        /// </summary>
        [MaxLength(200)]
        public string? Placeholder { get; set; }

        /// <summary>
        /// Help text / tooltip
        /// </summary>
        [MaxLength(500)]
        public string? HelpText { get; set; }

        /// <summary>
        /// Lookup/Foreign Key ise hangi entity'ye bağlı?
        /// </summary>
        [ForeignKey("RelatedEntity")]
        public Guid? RelatedEntityId { get; set; }
        public virtual FormEntity? RelatedEntity { get; set; }

        /// <summary>
        /// Lookup için display field (örn: FullName, CompanyName)
        /// </summary>
        [MaxLength(100)]
        public string? LookupDisplayField { get; set; }

        /// <summary>
        /// Lookup için value field (genelde Id)
        /// </summary>
        [MaxLength(100)]
        public string? LookupValueField { get; set; }

        /// <summary>
        /// Custom validation JSON (ek validation kuralları)
        /// </summary>
        public string? CustomValidationRules { get; set; }

        /// <summary>
        /// Bu alan için field mapping'ler
        /// </summary>
        public virtual ICollection<FormFieldMapping> FormFieldMappings { get; set; } = new List<FormFieldMapping>();
    }
}

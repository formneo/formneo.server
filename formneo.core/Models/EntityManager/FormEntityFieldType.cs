using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace formneo.core.Models.EntityManager
{
    /// <summary>
    /// Alan tiplerini tanımlar (String, Number, Date, Boolean, Lookup, etc.)
    /// </summary>
    public class FormEntityFieldType : BaseEntity
    {
        /// <summary>
        /// Tip adı (örn: String, Number, Date, Boolean, Lookup)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; }

        /// <summary>
        /// Tip açıklaması
        /// </summary>
        [MaxLength(500)]
        public string? TypeDescription { get; set; }

        /// <summary>
        /// C# veri tipi (örn: string, int, DateTime, bool)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string CSharpType { get; set; }

        /// <summary>
        /// TypeScript veri tipi (örn: string, number, Date, boolean)
        /// </summary>
        [MaxLength(200)]
        public string? TypeScriptType { get; set; }

        /// <summary>
        /// SQL Server veri tipi (örn: nvarchar, int, datetime, bit)
        /// </summary>
        [MaxLength(200)]
        public string? SqlServerType { get; set; }

        /// <summary>
        /// Default form component tipi (örn: Input, NumberInput, DatePicker, Checkbox)
        /// </summary>
        [MaxLength(200)]
        public string? DefaultComponentType { get; set; }

        /// <summary>
        /// Tip kategorisi (Primitive, Reference, Collection)
        /// </summary>
        [Required]
        public FieldTypeCategory Category { get; set; }

        /// <summary>
        /// Tip aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Sistem tarafından tanımlı mı? (built-in types)
        /// </summary>
        public bool IsSystemType { get; set; } = false;

        /// <summary>
        /// Validation options JSON (tip için geçerli validation seçenekleri)
        /// </summary>
        public string? ValidationOptions { get; set; }

        /// <summary>
        /// Component options JSON (tip için geçerli component seçenekleri)
        /// </summary>
        public string? ComponentOptions { get; set; }

        /// <summary>
        /// Bu tipi kullanan alanlar
        /// </summary>
        public virtual ICollection<FormEntityField> Fields { get; set; } = new List<FormEntityField>();
    }

    /// <summary>
    /// Alan tipi kategorileri
    /// </summary>
    public enum FieldTypeCategory
    {
        /// <summary>
        /// Primitive tipler (string, number, boolean, date)
        /// </summary>
        Primitive = 0,

        /// <summary>
        /// Referans tipler (lookup, foreign key)
        /// </summary>
        Reference = 1,

        /// <summary>
        /// Collection tipler (array, list)
        /// </summary>
        Collection = 2,

        /// <summary>
        /// Complex tipler (JSON, object)
        /// </summary>
        Complex = 3,

        /// <summary>
        /// File tipler (file, image, document)
        /// </summary>
        File = 4
    }
}

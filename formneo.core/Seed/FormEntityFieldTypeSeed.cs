using System;
using System.Collections.Generic;
using formneo.core.Models.EntityManager;

namespace formneo.core.Seed
{
    /// <summary>
    /// Form Entity Field Type için seed data
    /// Sistem için built-in field type'ları
    /// </summary>
    public static class FormEntityFieldTypeSeed
    {
        public static List<FormEntityFieldType> GetSeedData()
        {
            var seedData = new List<FormEntityFieldType>
            {
                // Primitive Types
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    TypeName = "String",
                    TypeDescription = "Metin verisi (kısa veya uzun)",
                    CSharpType = "string",
                    TypeScriptType = "string",
                    SqlServerType = "nvarchar",
                    DefaultComponentType = "Input",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"maxLength\":true,\"minLength\":true,\"regex\":true}",
                    ComponentOptions = "{\"multiline\":false,\"maxLength\":500}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    TypeName = "Text",
                    TypeDescription = "Uzun metin verisi (textarea)",
                    CSharpType = "string",
                    TypeScriptType = "string",
                    SqlServerType = "nvarchar(max)",
                    DefaultComponentType = "Input.TextArea",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"maxLength\":true,\"minLength\":true}",
                    ComponentOptions = "{\"multiline\":true,\"rows\":4}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    TypeName = "Integer",
                    TypeDescription = "Tam sayı verisi",
                    CSharpType = "int",
                    TypeScriptType = "number",
                    SqlServerType = "int",
                    DefaultComponentType = "NumberPicker",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"min\":true,\"max\":true}",
                    ComponentOptions = "{\"step\":1,\"precision\":0}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    TypeName = "Decimal",
                    TypeDescription = "Ondalık sayı verisi",
                    CSharpType = "decimal",
                    TypeScriptType = "number",
                    SqlServerType = "decimal(18,2)",
                    DefaultComponentType = "NumberPicker",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"min\":true,\"max\":true,\"precision\":true}",
                    ComponentOptions = "{\"step\":0.01,\"precision\":2}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    TypeName = "Boolean",
                    TypeDescription = "Evet/Hayır (true/false) verisi",
                    CSharpType = "bool",
                    TypeScriptType = "boolean",
                    SqlServerType = "bit",
                    DefaultComponentType = "Checkbox",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{}",
                    ComponentOptions = "{\"defaultChecked\":false}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    TypeName = "Date",
                    TypeDescription = "Tarih verisi (sadece gün/ay/yıl)",
                    CSharpType = "DateTime",
                    TypeScriptType = "Date",
                    SqlServerType = "date",
                    DefaultComponentType = "DatePicker",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"min\":true,\"max\":true}",
                    ComponentOptions = "{\"format\":\"DD/MM/YYYY\",\"showTime\":false}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000007"),
                    TypeName = "DateTime",
                    TypeDescription = "Tarih ve saat verisi",
                    CSharpType = "DateTime",
                    TypeScriptType = "Date",
                    SqlServerType = "datetime2",
                    DefaultComponentType = "DatePicker",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"min\":true,\"max\":true}",
                    ComponentOptions = "{\"format\":\"DD/MM/YYYY HH:mm\",\"showTime\":true}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000008"),
                    TypeName = "Time",
                    TypeDescription = "Saat verisi (sadece saat:dakika)",
                    CSharpType = "TimeSpan",
                    TypeScriptType = "string",
                    SqlServerType = "time",
                    DefaultComponentType = "TimePicker",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{}",
                    ComponentOptions = "{\"format\":\"HH:mm\"}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000009"),
                    TypeName = "Email",
                    TypeDescription = "Email adresi",
                    CSharpType = "string",
                    TypeScriptType = "string",
                    SqlServerType = "nvarchar(255)",
                    DefaultComponentType = "Input",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"email\":true}",
                    ComponentOptions = "{\"type\":\"email\",\"maxLength\":255}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    TypeName = "Phone",
                    TypeDescription = "Telefon numarası",
                    CSharpType = "string",
                    TypeScriptType = "string",
                    SqlServerType = "nvarchar(50)",
                    DefaultComponentType = "Input",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"phone\":true}",
                    ComponentOptions = "{\"type\":\"tel\",\"maxLength\":50}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    TypeName = "URL",
                    TypeDescription = "Web adresi",
                    CSharpType = "string",
                    TypeScriptType = "string",
                    SqlServerType = "nvarchar(2000)",
                    DefaultComponentType = "Input",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"url\":true}",
                    ComponentOptions = "{\"type\":\"url\",\"maxLength\":2000}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                    TypeName = "Guid",
                    TypeDescription = "Unique identifier (GUID/UUID)",
                    CSharpType = "Guid",
                    TypeScriptType = "string",
                    SqlServerType = "uniqueidentifier",
                    DefaultComponentType = "Input",
                    Category = FieldTypeCategory.Primitive,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"guid\":true}",
                    ComponentOptions = "{\"readOnly\":true}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },

                // Reference Types
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    TypeName = "Lookup",
                    TypeDescription = "Başka bir entity'ye referans (Foreign Key)",
                    CSharpType = "Guid",
                    TypeScriptType = "string",
                    SqlServerType = "uniqueidentifier",
                    DefaultComponentType = "Select",
                    Category = FieldTypeCategory.Reference,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"required\":true}",
                    ComponentOptions = "{\"mode\":\"default\",\"showSearch\":true}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    TypeName = "MultiLookup",
                    TypeDescription = "Birden fazla entity'ye referans (Many-to-Many)",
                    CSharpType = "List<Guid>",
                    TypeScriptType = "string[]",
                    SqlServerType = "nvarchar(max)",
                    DefaultComponentType = "Select",
                    Category = FieldTypeCategory.Reference,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"minCount\":true,\"maxCount\":true}",
                    ComponentOptions = "{\"mode\":\"multiple\",\"showSearch\":true}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },

                // Collection Types
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000030"),
                    TypeName = "StringArray",
                    TypeDescription = "Metin dizisi (string[])",
                    CSharpType = "List<string>",
                    TypeScriptType = "string[]",
                    SqlServerType = "nvarchar(max)",
                    DefaultComponentType = "Select",
                    Category = FieldTypeCategory.Collection,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"minCount\":true,\"maxCount\":true}",
                    ComponentOptions = "{\"mode\":\"tags\"}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                    TypeName = "NumberArray",
                    TypeDescription = "Sayı dizisi (number[])",
                    CSharpType = "List<int>",
                    TypeScriptType = "number[]",
                    SqlServerType = "nvarchar(max)",
                    DefaultComponentType = "Select",
                    Category = FieldTypeCategory.Collection,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"minCount\":true,\"maxCount\":true}",
                    ComponentOptions = "{\"mode\":\"multiple\"}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },

                // Complex Types
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000040"),
                    TypeName = "JSON",
                    TypeDescription = "JSON verisi (serbest format)",
                    CSharpType = "string",
                    TypeScriptType = "object",
                    SqlServerType = "nvarchar(max)",
                    DefaultComponentType = "Input.TextArea",
                    Category = FieldTypeCategory.Complex,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"json\":true}",
                    ComponentOptions = "{\"multiline\":true,\"rows\":6}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000041"),
                    TypeName = "Object",
                    TypeDescription = "Karmaşık nesne yapısı",
                    CSharpType = "object",
                    TypeScriptType = "object",
                    SqlServerType = "nvarchar(max)",
                    DefaultComponentType = "ObjectField",
                    Category = FieldTypeCategory.Complex,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{}",
                    ComponentOptions = "{}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },

                // File Types
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000050"),
                    TypeName = "File",
                    TypeDescription = "Dosya yükleme",
                    CSharpType = "string",
                    TypeScriptType = "string",
                    SqlServerType = "nvarchar(500)",
                    DefaultComponentType = "Upload",
                    Category = FieldTypeCategory.File,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"maxSize\":true,\"allowedTypes\":true}",
                    ComponentOptions = "{\"multiple\":false,\"maxCount\":1}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000051"),
                    TypeName = "Image",
                    TypeDescription = "Resim yükleme",
                    CSharpType = "string",
                    TypeScriptType = "string",
                    SqlServerType = "nvarchar(500)",
                    DefaultComponentType = "Upload",
                    Category = FieldTypeCategory.File,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"maxSize\":true,\"imageOnly\":true}",
                    ComponentOptions = "{\"multiple\":false,\"listType\":\"picture-card\",\"accept\":\"image/*\"}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new FormEntityFieldType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000052"),
                    TypeName = "MultiFile",
                    TypeDescription = "Çoklu dosya yükleme",
                    CSharpType = "List<string>",
                    TypeScriptType = "string[]",
                    SqlServerType = "nvarchar(max)",
                    DefaultComponentType = "Upload",
                    Category = FieldTypeCategory.File,
                    IsActive = true,
                    IsSystemType = true,
                    ValidationOptions = "{\"maxSize\":true,\"allowedTypes\":true,\"maxCount\":true}",
                    ComponentOptions = "{\"multiple\":true,\"maxCount\":10}",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            return seedData;
        }
    }
}

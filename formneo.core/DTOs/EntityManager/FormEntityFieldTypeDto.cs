using System;
using formneo.core.Models.EntityManager;

namespace formneo.core.DTOs.EntityManager
{
    public class FormEntityFieldTypeDto
    {
        public Guid Id { get; set; }
        public string TypeName { get; set; }
        public string? TypeDescription { get; set; }
        public string CSharpType { get; set; }
        public string? TypeScriptType { get; set; }
        public string? SqlServerType { get; set; }
        public string? DefaultComponentType { get; set; }
        public FieldTypeCategory Category { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemType { get; set; }
        public string? ValidationOptions { get; set; }
        public string? ComponentOptions { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FormEntityFieldTypeCreateDto
    {
        public string TypeName { get; set; }
        public string? TypeDescription { get; set; }
        public string CSharpType { get; set; }
        public string? TypeScriptType { get; set; }
        public string? SqlServerType { get; set; }
        public string? DefaultComponentType { get; set; }
        public FieldTypeCategory Category { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsSystemType { get; set; } = false;
        public string? ValidationOptions { get; set; }
        public string? ComponentOptions { get; set; }
    }

    public class FormEntityFieldTypeUpdateDto
    {
        public Guid Id { get; set; }
        public string TypeName { get; set; }
        public string? TypeDescription { get; set; }
        public string CSharpType { get; set; }
        public string? TypeScriptType { get; set; }
        public string? SqlServerType { get; set; }
        public string? DefaultComponentType { get; set; }
        public FieldTypeCategory Category { get; set; }
        public bool IsActive { get; set; }
        public string? ValidationOptions { get; set; }
        public string? ComponentOptions { get; set; }
    }

    public class FormEntityFieldTypeListDto
    {
        public Guid Id { get; set; }
        public string TypeName { get; set; }
        public string CSharpType { get; set; }
        public string? DefaultComponentType { get; set; }
        public string CategoryName { get; set; }
        public bool IsSystemType { get; set; }
    }
}

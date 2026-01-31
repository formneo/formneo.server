using System;

namespace formneo.core.DTOs.EntityManager
{
    public class FormFieldMappingDto
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public string? FormName { get; set; }
        public Guid FormEntityFieldId { get; set; }
        public string? FormEntityFieldName { get; set; }
        public string? FormEntityName { get; set; }
        public Guid? FormEntityRelationId { get; set; }
        public string? FormEntityRelationName { get; set; }
        public string FormElementId { get; set; }
        public string FormFieldName { get; set; }
        public string? FormComponentType { get; set; }
        public bool IsActive { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsAutoMapped { get; set; }
        public string? TransformRules { get; set; }
        public string? ValidationOverride { get; set; }
        public int DisplayOrder { get; set; }
        public string? MappingNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FormFieldMappingCreateDto
    {
        public Guid FormId { get; set; }
        public Guid FormEntityFieldId { get; set; }
        public Guid? FormEntityRelationId { get; set; }
        public string FormElementId { get; set; }
        public string FormFieldName { get; set; }
        public string? FormComponentType { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsReadOnly { get; set; } = false;
        public bool IsAutoMapped { get; set; } = false;
        public string? TransformRules { get; set; }
        public string? ValidationOverride { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public string? MappingNotes { get; set; }
    }

    public class FormFieldMappingUpdateDto
    {
        public Guid Id { get; set; }
        public Guid FormEntityFieldId { get; set; }
        public Guid? FormEntityRelationId { get; set; }
        public string FormElementId { get; set; }
        public string FormFieldName { get; set; }
        public string? FormComponentType { get; set; }
        public bool IsActive { get; set; }
        public bool IsReadOnly { get; set; }
        public string? TransformRules { get; set; }
        public string? ValidationOverride { get; set; }
        public int DisplayOrder { get; set; }
        public string? MappingNotes { get; set; }
    }

    public class FormFieldMappingListDto
    {
        public Guid Id { get; set; }
        public string FormFieldName { get; set; }
        public string? FormEntityFieldName { get; set; }
        public string? FormEntityName { get; set; }
        public bool IsActive { get; set; }
        public bool IsAutoMapped { get; set; }
    }

    /// <summary>
    /// Auto-mapping için kullanılacak DTO
    /// Form design'dan otomatik mapping oluşturur
    /// </summary>
    public class AutoMapFormFieldsDto
    {
        public Guid FormId { get; set; }
        public Guid FormEntityId { get; set; }
        public Guid? FormEntityRelationId { get; set; }
        public bool OverwriteExisting { get; set; } = false;
        public bool MapOnlyUnmapped { get; set; } = true;
    }
}

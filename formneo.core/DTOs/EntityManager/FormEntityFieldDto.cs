using System;
using System.Collections.Generic;

namespace formneo.core.DTOs.EntityManager
{
    public class FormEntityFieldDto
    {
        public Guid Id { get; set; }
        public Guid FormEntityId { get; set; }
        public string? FormEntityName { get; set; }
        public string FieldName { get; set; }
        public string? FieldDescription { get; set; }
        public Guid FieldTypeId { get; set; }
        public string? FieldTypeName { get; set; }
        public string? ColumnName { get; set; }
        public string? PropertyName { get; set; }
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsIndexed { get; set; }
        public bool IsNullable { get; set; }
        public bool IsActive { get; set; }
        public int? MaxLength { get; set; }
        public int? MinLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? DefaultValue { get; set; }
        public string? RegexPattern { get; set; }
        public string? RegexErrorMessage { get; set; }
        public int DisplayOrder { get; set; }
        public string? DisplayLabel { get; set; }
        public string? Placeholder { get; set; }
        public string? HelpText { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityName { get; set; }
        public string? LookupDisplayField { get; set; }
        public string? LookupValueField { get; set; }
        public string? CustomValidationRules { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class FormEntityFieldCreateDto
    {
        public Guid FormEntityId { get; set; }
        public string FieldName { get; set; }
        public string? FieldDescription { get; set; }
        public Guid FieldTypeId { get; set; }
        public string? ColumnName { get; set; }
        public string? PropertyName { get; set; }
        public bool IsRequired { get; set; } = false;
        public bool IsUnique { get; set; } = false;
        public bool IsIndexed { get; set; } = false;
        public bool IsNullable { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int? MaxLength { get; set; }
        public int? MinLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? DefaultValue { get; set; }
        public string? RegexPattern { get; set; }
        public string? RegexErrorMessage { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public string? DisplayLabel { get; set; }
        public string? Placeholder { get; set; }
        public string? HelpText { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? LookupDisplayField { get; set; }
        public string? LookupValueField { get; set; }
        public string? CustomValidationRules { get; set; }
    }

    public class FormEntityFieldUpdateDto
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; }
        public string? FieldDescription { get; set; }
        public Guid FieldTypeId { get; set; }
        public string? ColumnName { get; set; }
        public string? PropertyName { get; set; }
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsIndexed { get; set; }
        public bool IsNullable { get; set; }
        public bool IsActive { get; set; }
        public int? MaxLength { get; set; }
        public int? MinLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? DefaultValue { get; set; }
        public string? RegexPattern { get; set; }
        public string? RegexErrorMessage { get; set; }
        public int DisplayOrder { get; set; }
        public string? DisplayLabel { get; set; }
        public string? Placeholder { get; set; }
        public string? HelpText { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? LookupDisplayField { get; set; }
        public string? LookupValueField { get; set; }
        public string? CustomValidationRules { get; set; }
    }

    public class FormEntityFieldListDto
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; }
        public string? FieldTypeName { get; set; }
        public string? DisplayLabel { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}

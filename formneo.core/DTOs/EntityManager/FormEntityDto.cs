using System;
using System.Collections.Generic;

namespace formneo.core.DTOs.EntityManager
{
    public class FormEntityDto
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; }
        public string? EntityDescription { get; set; }
        public string? TableName { get; set; }
        public string? SchemaName { get; set; }
        public string? NamespacePath { get; set; }
        public string? ClassName { get; set; }
        public bool IsActive { get; set; }
        public bool AllowCreate { get; set; }
        public bool AllowRead { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? DisplayField { get; set; }
        public string? OrderByField { get; set; }
        public Guid? ParentEntityId { get; set; }
        public string? ParentEntityName { get; set; }
        public List<FormEntityFieldDto>? Fields { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class FormEntityCreateDto
    {
        public string EntityName { get; set; }
        public string? EntityDescription { get; set; }
        public string? TableName { get; set; }
        public string? SchemaName { get; set; }
        public string? NamespacePath { get; set; }
        public string? ClassName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool AllowCreate { get; set; } = true;
        public bool AllowRead { get; set; } = true;
        public bool AllowUpdate { get; set; } = true;
        public bool AllowDelete { get; set; } = false;
        public string? ApiEndpoint { get; set; }
        public string? DisplayField { get; set; }
        public string? OrderByField { get; set; }
        public Guid? ParentEntityId { get; set; }
    }

    public class FormEntityUpdateDto
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; }
        public string? EntityDescription { get; set; }
        public string? TableName { get; set; }
        public string? SchemaName { get; set; }
        public string? NamespacePath { get; set; }
        public string? ClassName { get; set; }
        public bool IsActive { get; set; }
        public bool AllowCreate { get; set; }
        public bool AllowRead { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? DisplayField { get; set; }
        public string? OrderByField { get; set; }
        public Guid? ParentEntityId { get; set; }
    }

    public class FormEntityListDto
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; }
        public string? EntityDescription { get; set; }
        public string? TableName { get; set; }
        public bool IsActive { get; set; }
        public int FieldCount { get; set; }
        public int FormCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

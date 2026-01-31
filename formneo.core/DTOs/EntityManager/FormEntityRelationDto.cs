using System;
using formneo.core.Models.EntityManager;

namespace formneo.core.DTOs.EntityManager
{
    public class FormEntityRelationDto
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public string? FormName { get; set; }
        public Guid FormEntityId { get; set; }
        public string? FormEntityName { get; set; }
        public string RelationName { get; set; }
        public string? RelationDescription { get; set; }
        public EntityRelationType RelationType { get; set; }
        public string RelationTypeName { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public bool CascadeDelete { get; set; }
        public int DisplayOrder { get; set; }
        public Guid? ParentRelationId { get; set; }
        public string? ParentRelationName { get; set; }
        public string? FormDataPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FormEntityRelationCreateDto
    {
        public Guid FormId { get; set; }
        public Guid FormEntityId { get; set; }
        public string RelationName { get; set; }
        public string? RelationDescription { get; set; }
        public EntityRelationType RelationType { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsRequired { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public bool CascadeDelete { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public Guid? ParentRelationId { get; set; }
        public string? FormDataPath { get; set; }
    }

    public class FormEntityRelationUpdateDto
    {
        public Guid Id { get; set; }
        public string RelationName { get; set; }
        public string? RelationDescription { get; set; }
        public EntityRelationType RelationType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public bool CascadeDelete { get; set; }
        public int DisplayOrder { get; set; }
        public Guid? ParentRelationId { get; set; }
        public string? FormDataPath { get; set; }
    }

    public class FormEntityRelationListDto
    {
        public Guid Id { get; set; }
        public string RelationName { get; set; }
        public string? FormEntityName { get; set; }
        public string RelationTypeName { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }
}

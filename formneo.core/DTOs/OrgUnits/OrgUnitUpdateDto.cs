using System;
using formneo.core.Models;

namespace formneo.core.DTOs.OrgUnits
{
    public class OrgUnitUpdateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; }
        public OrgUnitType Type { get; set; } = OrgUnitType.Department;
        public bool IsActive { get; set; } = true;
        public string? ManagerId { get; set; }
        public Guid? ParentOrgUnitId { get; set; }
    }
}


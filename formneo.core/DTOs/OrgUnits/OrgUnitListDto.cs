using System;
using System.Collections.Generic;
using formneo.core.Models;

namespace formneo.core.DTOs.OrgUnits
{
    public class OrgUnitListDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; }
        public OrgUnitType Type { get; set; }
        public bool IsActive { get; set; }
        public string? ManagerId { get; set; }
        public UserApp? Manager { get; set; }
        public Guid? ParentOrgUnitId { get; set; }
        public List<OrgUnit> SubOrgUnits { get; set; } = new List<OrgUnit>();
    }
}


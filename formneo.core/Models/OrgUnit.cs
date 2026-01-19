using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace formneo.core.Models
{
    public class OrgUnit : BaseEntity
    {
        public string Name { get; set; }
        public string? Code { get; set; }
        public OrgUnitType Type { get; set; } = OrgUnitType.Department;
        public bool IsActive { get; set; } = true;

        public Guid? ParentOrgUnitId { get; set; }
        [ForeignKey("ParentOrgUnitId")]
        public virtual OrgUnit? ParentOrgUnit { get; set; }
        public virtual List<OrgUnit> SubOrgUnits { get; set; } = new List<OrgUnit>();

        public string? ManagerId { get; set; }
        [ForeignKey("ManagerId")]
        public virtual UserApp? Manager { get; set; }

        // NOT: Users navigation property kaldırıldı
        // Kullanıcılar artık EmployeeAssignment tablosu üzerinden OrgUnit'e bağlanıyor
    }

    public enum OrgUnitType
    {
        Department = 1,
        Team = 2,
        Branch = 3,
        CostCenter = 4,
        Other = 99
    }
}


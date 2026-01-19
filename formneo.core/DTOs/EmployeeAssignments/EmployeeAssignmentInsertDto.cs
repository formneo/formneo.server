using System;
using System.ComponentModel.DataAnnotations;
using formneo.core.Models;

namespace formneo.core.DTOs.EmployeeAssignments
{
    public class EmployeeAssignmentInsertDto
    {
        [Required]
        public string UserId { get; set; }
        
        public Guid? OrgUnitId { get; set; }
        
        public Guid? PositionId { get; set; }
        
        public string? ManagerId { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public AssignmentType AssignmentType { get; set; } = AssignmentType.Primary;
        
        public string? Notes { get; set; }
    }
}



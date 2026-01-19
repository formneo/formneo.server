using System;
using formneo.core.Models;

namespace formneo.core.DTOs.EmployeeAssignments
{
    public class 
        EmployeeAssignmentListDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        
        public Guid? OrgUnitId { get; set; }
        public string? OrgUnitName { get; set; }
        
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        
        public string? ManagerId { get; set; }
        public string? ManagerFullName { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public bool IsActive { get; set; }
        
        public AssignmentType AssignmentType { get; set; }
        public string AssignmentTypeText { get; set; }
        public string? Notes { get; set; }
        
        public DateTime CreatedDate { get; set; }
    }
}


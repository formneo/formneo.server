using System;
using System.Collections.Generic;

namespace formneo.core.DTOs
{
    /// <summary>
    /// Workflow menü yapısı root DTO
    /// </summary>
    public class WorkFlowMenuResponseDto
    {
        public List<WorkFlowMenuGroupDto> Menus { get; set; } = new List<WorkFlowMenuGroupDto>();
    }

    /// <summary>
    /// Workflow menü grubu (örn: "Süreçler")
    /// </summary>
    public class WorkFlowMenuGroupDto
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public List<WorkFlowMenuItemDto> Children { get; set; } = new List<WorkFlowMenuItemDto>();
    }

    /// <summary>
    /// Workflow menü item'ı (bir workflow)
    /// </summary>
    public class WorkFlowMenuItemDto
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public string WorkflowId { get; set; }
        public Guid WorkflowGuid { get; set; }
        public int Revision { get; set; }
        public Guid? FormId { get; set; }
        public string? FormName { get; set; }
        public int? FormRevision { get; set; }
        public List<WorkFlowMenuViewDto> Views { get; set; } = new List<WorkFlowMenuViewDto>();
    }

    /// <summary>
    /// Workflow view'ları (Benim Taleplerim, Onay Bekleyenler vb.)
    /// </summary>
    public class WorkFlowMenuViewDto
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Path { get; set; }
    }
}

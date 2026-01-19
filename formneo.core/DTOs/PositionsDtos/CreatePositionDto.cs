using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace formneo.core.DTOs.PositionsDtos
{
    public class CreatePositionDto
    {
        public string Name { get; set; }
        public string? Description { get; set; } = null;
        public Guid? ParentPositionId { get; set; } = null;
    }
}

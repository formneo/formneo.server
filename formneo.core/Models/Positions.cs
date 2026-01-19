using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using formneo.core.Models.Ticket;

namespace formneo.core.Models
{
    public class Positions:BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; } = null;

        public Guid? ParentPositionId { get; set; }
        [ForeignKey("ParentPositionId")]
        public virtual Positions? ParentPosition { get; set; }
        public virtual List<Positions> SubPositions { get; set; } = new List<Positions>();

        public virtual List<UserApp> UserApps { get; set; } = new List<UserApp>();


    }
}

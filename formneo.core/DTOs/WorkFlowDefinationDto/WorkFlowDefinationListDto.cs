using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.Models;

namespace formneo.core.DTOs
{
    public class WorkFlowDefinationListDto
    {


        public Guid Id { get; set; }
        public string? WorkflowName { get; set; }
        public Boolean IsActive { get; set; }

        public int Revision { get; set; }

        public Guid? FormId { get; set; }
        
        public string? FormName { get; set; }
        
        public int? FormRevision { get; set; }


    }
    
    public class WorkFlowDefinationDetailDto
    {


        public Guid Id { get; set; }
        public string? WorkflowName { get; set; }
        public string Defination { get; set; }

        public Boolean IsActive { get; set; }

        public int Revision { get; set; }

        public Guid? FormId { get; set; }
        
        public string? FormName { get; set; }
        
        public int? FormRevision { get; set; }


    }
    
    public class WorkFlowDefinationWithInitScriptDto
    {
        public Guid Id { get; set; }
        public string? WorkflowName { get; set; }
        public Boolean IsActive { get; set; }
        public int Revision { get; set; }
        public Guid? FormId { get; set; }
        public string? FormName { get; set; }
        public int? FormRevision { get; set; }
        
        // Tüm form node'larının initScript'lerini birleştirilmiş hali
        public string? InitScript { get; set; }
    }
}

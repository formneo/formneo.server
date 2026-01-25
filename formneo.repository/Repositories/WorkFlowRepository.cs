using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.Models;
using formneo.core.Repositories;

namespace formneo.repository.Repositories
{
    public class WorkFlowRepository : GenericRepository<WorkflowHead>, IWorkflowRepository
    {
        public WorkFlowRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<WorkflowHead> GetWorkFlowWitId(Guid id)
        {
            var workflowHead = await _context.WorkflowHead
                .Include(e => e.workflowItems)
                    .ThenInclude(wi => wi.formItems)  // ✅ FormItems'ları da yükle!
                .Include(e => e.workflowItems)
                    .ThenInclude(wi => wi.approveItems)  // ✅ ApproveItems'ları da yükle!
                .FirstOrDefaultAsync(x => x.Id == id);
            return workflowHead;
        }
    }
}

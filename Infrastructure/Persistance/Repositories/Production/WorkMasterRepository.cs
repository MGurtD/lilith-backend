using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Production;
using Application.Persistance.Repositories.Sales;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkMasterRepository : Repository<WorkMaster, Guid>, IWorkMasterRepository
    {
        public IWorkMasterPhaseRepository Phases { get; }

        public WorkMasterRepository(ApplicationDbContext context) : base(context)
        {
            Phases = new WorkMasterPhaseRepository(context);
        }

        public override async Task<WorkMaster?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Phases)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}

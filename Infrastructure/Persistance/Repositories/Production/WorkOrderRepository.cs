using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkOrderRepository : Repository<WorkOrder, Guid>, IWorkOrderRepository
    {
        public IWorkOrderPhaseRepository Phases { get; }

        public WorkOrderRepository(ApplicationDbContext context) : base(context)
        {
            Phases = new WorkOrderPhaseRepository(context);
        }

        public override async Task<IEnumerable<WorkOrder>> GetAll()
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .AsNoTracking()
                        .ToListAsync();
        }

        public override async Task<WorkOrder?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include(d => d.Phases)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

 
    }
}

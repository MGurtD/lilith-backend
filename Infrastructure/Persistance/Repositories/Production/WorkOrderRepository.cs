using System.Linq.Expressions;
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

        public override IEnumerable<WorkOrder> Find(Expression<Func<WorkOrder, bool>> predicate) {
            return dbSet
                    .Include(d => d.Reference)
                    .Where(predicate)
                    .AsNoTracking()
                    .OrderBy(w => w.Code);
        }

        public override async Task<WorkOrder?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include(d => d.Phases)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<WorkOrder>> GetByWorkcenterIdInProduction(Guid workcenterId, Guid productionStatusId)
        {
            return await dbSet
                .Include(w => w.Reference)
                    .ThenInclude(r => r!.Customer)
                .Include(w => w.Phases.Where(p => 
                    p.WorkcenterTypeId == workcenterId || p.PreferredWorkcenterId == workcenterId))
                .Where(w => w.StatusId == productionStatusId && 
                            w.Phases.Any(p => 
                                p.WorkcenterTypeId == workcenterId || p.PreferredWorkcenterId == workcenterId))
                .AsNoTracking()
                .OrderBy(w => w.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkOrder>> GetByWorkcenterType(Guid workcenterTypeId, Guid[] excludedStatusIds)
        {
            return await dbSet
                .Where(w => !excludedStatusIds.Contains(w.StatusId))
                .Where(w => w.Phases.Any(p => p.WorkcenterTypeId == workcenterTypeId))
                .Include(w => w.Reference)
                    .ThenInclude(r => r!.Customer)
                .Include(w => w.Status)
                .Include(w => w.Phases.Where(p => p.WorkcenterTypeId == workcenterTypeId))                   
                .AsNoTracking()
                .OrderBy(w => w.Order)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrders(List<UpdateWorkOrderOrderDTO> orders)
        {
            var ids = orders.Select(o => o.Id).ToArray();
            var workOrders = await dbSet.Where(w => ids.Contains(w.Id)).ToListAsync();
            if (workOrders.Count != ids.Length)
                return false;
            
            foreach (var workOrder in workOrders)
            {
                var order = orders.First(o => o.Id == workOrder.Id);
                workOrder.Order = order.Order;
                UpdateWithoutSave(workOrder);
            }
            await SaveChanges();
            return true;
        }


    }
}

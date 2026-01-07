using System.Linq.Expressions;
using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Shared;
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

        public async Task<WorkOrder?> GetDetailed(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include(d => d.Phases)
                            .ThenInclude(p => p.Details)
                        .Include(d => d.Phases)
                            .ThenInclude(d => d.BillOfMaterials)
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
        /// <summary>
        /// Gets work orders with planned phases for a specific workcenter type.
        /// Uses efficient EF Core query with proper includes for hierarchical data.
        /// </summary>
        public async Task<(IEnumerable<WorkOrder> workOrders, Dictionary<Guid, string> statusLookup)> 
            GetWorkOrdersWithPlannedPhases(Guid workcenterTypeId)
        {
            // Step 1: Get phase IDs with Planned lifecycle tag and their statuses
            var phaseDataQuery = from phase in context.Set<WorkOrderPhase>()
                                 join status in context.Set<Status>() 
                                     on phase.StatusId equals status.Id
                                 join statusTag in context.Set<StatusLifecycleTag>() 
                                     on status.Id equals statusTag.StatusId
                                 join tag in context.Set<LifecycleTag>() 
                                     on statusTag.LifecycleTagId equals tag.Id
                                 where phase.WorkcenterTypeId == workcenterTypeId
                                     && tag.Name == StatusConstants.LifecycleTags.Planned
                                     && !tag.Disabled
                                     && !statusTag.Disabled
                                     && !status.Disabled
                                     && !phase.Disabled
                                 select new 
                                 { 
                                     PhaseId = phase.Id,
                                     phase.WorkOrderId,
                                     StatusName = status.Name
                                 };

            var phaseData = await phaseDataQuery.ToListAsync();
            
            if (phaseData.Count == 0)
                return (Enumerable.Empty<WorkOrder>(), new Dictionary<Guid, string>());

            var phaseIds = phaseData.Select(p => p.PhaseId).ToArray();
            var workOrderIds = phaseData.Select(p => p.WorkOrderId).Distinct().ToArray();

            // Step 2: Load WorkOrders with all navigation properties in single query
            var workOrders = await dbSet
                .Where(wo => workOrderIds.Contains(wo.Id) && !wo.Disabled)
                .Include(wo => wo.Status)
                .Include(wo => wo.Reference)
                    .ThenInclude(r => r!.Customer)
                .Include(wo => wo.Phases.Where(p => phaseIds.Contains(p.Id)))
                    .ThenInclude(p => p.PreferredWorkcenter)
                .AsNoTracking()
                .OrderBy(w => w.Order)
                    .ThenBy(w => w.PlannedDate)
                .ToListAsync();

            // Step 3: Build status lookup
            var statusLookup = phaseData.ToDictionary(p => p.PhaseId, p => p.StatusName);

            return (workOrders, statusLookup);
        }

        public async Task<WorkOrder?> GetWorkOrderWithPhasesDetailed(Guid workOrderId)
        {
            return await dbSet
                .Include(wo => wo.Reference)
                    .ThenInclude(r => r!.Customer)
                .Include(wo => wo.Phases)
                    .ThenInclude(p => p.Status)
                .Include(wo => wo.Phases)
                    .ThenInclude(p => p.PreferredWorkcenter)
                .Include(wo => wo.Phases)
                    .ThenInclude(p => p.Details)
                        .ThenInclude(d => d.MachineStatus)
                .Include(wo => wo.Phases)
                    .ThenInclude(p => p.BillOfMaterials)
                        .ThenInclude(bom => bom.Reference)
                .AsNoTracking()
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId);
        }

        /// <summary>
        /// Gets work orders by loaded phase IDs with efficient single EF Core query.
        /// Includes all navigation properties needed for DTO transformation.
        /// </summary>
        public async Task<IEnumerable<WorkOrder>> GetWorkOrdersByLoadedPhaseIds(List<Guid> phaseIds)
        {
            if (phaseIds == null || phaseIds.Count == 0)
                return [];

            // Single efficient query with all necessary includes
            var workOrders = await dbSet
                .Where(wo => wo.Phases.Any(p => phaseIds.Contains(p.Id)) && !wo.Disabled)
                .Include(wo => wo.Status)
                .Include(wo => wo.Reference)
                    .ThenInclude(r => r!.Customer)
                .Include(wo => wo.Phases.Where(p => phaseIds.Contains(p.Id)))
                    .ThenInclude(p => p.Status)
                .Include(wo => wo.Phases.Where(p => phaseIds.Contains(p.Id)))
                    .ThenInclude(p => p.PreferredWorkcenter)
                .AsNoTracking()
                .OrderBy(w => w.Order)
                    .ThenBy(w => w.PlannedDate)
                .ToListAsync();

            return workOrders;
        }

    }
}

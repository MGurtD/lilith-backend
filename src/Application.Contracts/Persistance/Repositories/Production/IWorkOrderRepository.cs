using Application.Contracts.Contracts.Production;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkOrderRepository : IRepository<WorkOrder, Guid>
    {
        
        IWorkOrderPhaseRepository Phases { get; }

        Task<WorkOrder?> GetDetailed(Guid id);
        Task<IEnumerable<WorkOrder>> GetByWorkcenterIdInProduction(Guid workcenterId, Guid productionStatusId);

        Task<IEnumerable<WorkOrder>> GetByWorkcenterType(Guid workcenterTypeId, Guid[] excludedStatusIds);
        Task<bool> UpdateOrders(List<UpdateWorkOrderOrderDTO> orders);

        /// <summary>
        /// Gets work orders with planned phases efficiently using EF Core includes.
        /// Returns entities and status lookup for DTO transformation in service layer.
        /// </summary>
        Task<(IEnumerable<WorkOrder> workOrders, Dictionary<Guid, string> statusLookup)> 
            GetWorkOrdersWithPlannedPhases(Guid workcenterTypeId);

        /// <summary>
        /// Gets work orders by loaded phase IDs with efficient EF Core query.
        /// Loads all related data (phases, reference, customer, status) in a single query.
        /// Used for WebSocket workcenter updates to display loaded work order information.
        /// </summary>
        /// <param name="phaseIds">List of work order phase IDs currently loaded on workcenter</param>
        /// <returns>Work orders with their loaded phases and all navigation properties</returns>
        Task<IEnumerable<WorkOrder>> GetWorkOrdersByLoadedPhaseIds(List<Guid> phaseIds);

        /// <summary>
        /// Gets a work order with all phases including detailed information (phase details, BOM).
        /// Used for Step 2 of work order selection process.
        /// </summary>
        Task<WorkOrder?> GetWorkOrderWithPhasesDetailed(Guid workOrderId);

    }
}

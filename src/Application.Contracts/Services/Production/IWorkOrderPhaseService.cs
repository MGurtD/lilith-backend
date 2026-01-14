using Application.Contracts;
using Application.Contracts.Contracts.Production;
using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkOrderPhaseService
{
    // Phase CRUD
    Task<WorkOrderPhase?> GetById(Guid id);
    Task<GenericResponse> Create(WorkOrderPhase phase);
    Task<GenericResponse> Update(WorkOrderPhase phase);
    Task<GenericResponse> Remove(Guid id);
    
    // Phase Lifecycle
    Task<GenericResponse> StartPhase(Guid phaseId, Guid? workOrderStatusId = null);
    Task<GenericResponse> EndPhase(Guid phaseId, Guid workOrderStatusId);
    
    // Special queries
    Task<IEnumerable<object>> GetExternalPhases(DateTime startTime, DateTime endTime);
    /// <summary>
    /// Gets planned work order phases grouped by work order for a specific workcenter type.
    /// Returns hierarchical structure for TreeTable display.
    /// </summary>
    /// <param name="workcenterTypeId">Workcenter type ID to filter phases</param>
    /// <returns>List of work orders with their planned phases</returns>
    Task<IEnumerable<WorkOrderWithPhasesDto>> GetPlannedByWorkcenterType(Guid workcenterTypeId);
    
    /// <summary>
    /// Gets detailed work order information for phases currently loaded on a workcenter.
    /// Returns work orders with customer, reference, and phase details based on phase IDs from WebSocket.
    /// </summary>
    /// <param name="phaseIds">List of work order phase IDs currently loaded</param>
    /// <returns>List of work orders with their phase information. Returns empty list if phases not found.</returns>
    Task<IEnumerable<WorkOrderWithPhasesDto>> GetLoadedWorkOrdersByPhaseIds(List<Guid> phaseIds);
    
    /// <summary>
    /// Gets detailed phase information for a specific work order including phase details and bill of materials.
    /// Used for Step 2 of work order selection process in plant module.
    /// </summary>
    /// <param name="workOrderId">Work order ID</param>
    /// <returns>List of phases with detailed information</returns>
    Task<IEnumerable<WorkOrderPhaseDetailedDto>> GetWorkOrderPhasesDetailed(Guid workOrderId);
    
    // PhaseDetail CRUD
    Task<WorkOrderPhaseDetail?> GetDetailById(Guid id);
    Task<GenericResponse> CreateDetail(WorkOrderPhaseDetail detail);
    Task<GenericResponse> UpdateDetail(WorkOrderPhaseDetail detail);
    Task<GenericResponse> RemoveDetail(Guid id);
    
    // BillOfMaterials CRUD
    Task<WorkOrderPhaseBillOfMaterials?> GetBillOfMaterialsById(Guid id);
    Task<GenericResponse> CreateBillOfMaterials(WorkOrderPhaseBillOfMaterials item);
    Task<GenericResponse> UpdateBillOfMaterials(WorkOrderPhaseBillOfMaterials item);
    Task<GenericResponse> RemoveBillOfMaterials(Guid id);
}


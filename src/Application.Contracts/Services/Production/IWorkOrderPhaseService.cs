using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkOrderPhaseService
{
    // Phase CRUD
    Task<WorkOrderPhase?> GetById(Guid id);
    Task<GenericResponse> Create(WorkOrderPhase phase);
    Task<GenericResponse> Update(WorkOrderPhase phase);
    Task<GenericResponse> Remove(Guid id);
    
    // Special queries
    Task<IEnumerable<object>> GetExternalPhases(DateTime startTime, DateTime endTime);
    
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

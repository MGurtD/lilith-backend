using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production;

public class WorkOrderPhaseService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IWorkOrderPhaseService
{
    #region Phase CRUD
    
    public async Task<WorkOrderPhase?> GetById(Guid id)
    {
        return await unitOfWork.WorkOrders.Phases.Get(id);
    }

    public async Task<GenericResponse> Create(WorkOrderPhase phase)
    {
        // Check if phase already exists
        var exists = unitOfWork.WorkOrders.Phases.Find(p => p.Id == phase.Id).Any();
        if (exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseAlreadyExists"));

        // Validate parent WorkOrder exists
        var workOrder = await unitOfWork.WorkOrders.Get(phase.WorkOrderId);
        if (workOrder is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderNotFound", phase.WorkOrderId));

        // Create phase
        await unitOfWork.WorkOrders.Phases.Add(phase);
        return new GenericResponse(true, phase);
    }

    public async Task<GenericResponse> Update(WorkOrderPhase phase)
    {
        // Check if phase exists
        var exists = await unitOfWork.WorkOrders.Phases.Exists(phase.Id);
        if (!exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));

        // Update phase
        await unitOfWork.WorkOrders.Phases.Update(phase);
        return new GenericResponse(true, phase);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.WorkOrders.Phases.Find(p => p.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));

        await unitOfWork.WorkOrders.Phases.Remove(entity);
        return new GenericResponse(true, entity);
    }
    
    #endregion

    #region Special Queries
    
    public async Task<IEnumerable<object>> GetExternalPhases(DateTime startTime, DateTime endTime)
    {
        // Get production status
        var status = await unitOfWork.Lifecycles.GetStatusByName(
            StatusConstants.Lifecycles.WorkOrder, 
            StatusConstants.Statuses.Production);
        
        if (status == null)
        {
            return [];
        }

        // Find work orders in production within date range
        var workOrders = await unitOfWork.WorkOrders.FindAsync(w =>
            w.StatusId == status.Id &&
            w.PlannedDate >= startTime &&
            w.PlannedDate < endTime);

        // Find external work phases without purchase orders
        var workOrderPhases = await unitOfWork.WorkOrders.Phases.FindAsync(p =>
            p.IsExternalWork == true &&
            p.ServiceReferenceId != null &&
            p.PurchaseOrderId == null);

        // Join phases with work orders
        var workOrderPhaseJoin = from w in workOrders
                                 join p in workOrderPhases
                                 on w.Id equals p.WorkOrderId
                                 select new
                                 {
                                     WorkOrder = w,
                                     Phase = p
                                 };

        return workOrderPhaseJoin;
    }
    
    #endregion

    #region PhaseDetail CRUD
    
    public async Task<WorkOrderPhaseDetail?> GetDetailById(Guid id)
    {
        return await unitOfWork.WorkOrders.Phases.Details.Get(id);
    }

    public async Task<GenericResponse> CreateDetail(WorkOrderPhaseDetail detail)
    {
        // Check if detail already exists
        var exists = unitOfWork.WorkOrders.Phases.Details.Find(d => d.Id == detail.Id).Any();
        if (exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailAlreadyExists"));

        // Create detail
        await unitOfWork.WorkOrders.Phases.Details.Add(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> UpdateDetail(WorkOrderPhaseDetail detail)
    {
        // Check if detail exists
        var exists = await unitOfWork.WorkOrders.Phases.Details.Exists(detail.Id);
        if (!exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound"));

        // Update detail
        await unitOfWork.WorkOrders.Phases.Details.Update(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> RemoveDetail(Guid id)
    {
        var entity = unitOfWork.WorkOrders.Phases.Details.Find(d => d.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound"));

        await unitOfWork.WorkOrders.Phases.Details.Remove(entity);
        return new GenericResponse(true, entity);
    }
    
    #endregion

    #region BillOfMaterials CRUD
    
    public async Task<WorkOrderPhaseBillOfMaterials?> GetBillOfMaterialsById(Guid id)
    {
        return await unitOfWork.WorkOrders.Phases.BillOfMaterials.Get(id);
    }

    public async Task<GenericResponse> CreateBillOfMaterials(WorkOrderPhaseBillOfMaterials item)
    {
        // Check if item already exists
        var exists = unitOfWork.WorkOrders.Phases.BillOfMaterials.Find(b => b.Id == item.Id).Any();
        if (exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailAlreadyExists"));

        // Create item
        await unitOfWork.WorkOrders.Phases.BillOfMaterials.Add(item);
        return new GenericResponse(true, item);
    }

    public async Task<GenericResponse> UpdateBillOfMaterials(WorkOrderPhaseBillOfMaterials item)
    {
        // Check if item exists
        var exists = await unitOfWork.WorkOrders.Phases.BillOfMaterials.Exists(item.Id);
        if (!exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound"));

        // Update item
        await unitOfWork.WorkOrders.Phases.BillOfMaterials.Update(item);
        return new GenericResponse(true, item);
    }

    public async Task<GenericResponse> RemoveBillOfMaterials(Guid id)
    {
        var entity = unitOfWork.WorkOrders.Phases.BillOfMaterials.Find(b => b.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound"));

        await unitOfWork.WorkOrders.Phases.BillOfMaterials.Remove(entity);
        return new GenericResponse(true, entity);
    }
    
    #endregion
}

using Application.Contracts;
using Application.Contracts.Contracts.Production;
using Domain.Entities.Production;
using Microsoft.Extensions.Logging;

namespace Application.Services.Production;

public class WorkOrderPhaseService(
    IUnitOfWork unitOfWork, 
    ILocalizationService localizationService,
    ILogger<WorkOrderPhaseService> logger) : IWorkOrderPhaseService
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

    #region Phase Lifecycle
    
    public async Task<GenericResponse> StartPhase(Guid phaseId)
    {
        // Get phase
        var phase = await unitOfWork.WorkOrders.Phases.Get(phaseId);
        if (phase == null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));
        
        // Get "Producció" status
        var productionStatus = await unitOfWork.Lifecycles.GetStatusByName(
            StatusConstants.Lifecycles.WorkOrder, 
            StatusConstants.Statuses.Production);
        
        if (productionStatus == null)
        {
            logger.LogError("Production status not found in WorkOrder lifecycle");
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.Production));
        }
        
        // Update phase (reset EndTime if it was previously finished)
        phase.StartTime = DateTime.Now;
        phase.EndTime = null;  // Reset to reopen the phase
        phase.StatusId = productionStatus.Id;
        unitOfWork.WorkOrders.Phases.UpdateWithoutSave(phase);
        
        // Update parent WorkOrder
        var workOrder = await unitOfWork.WorkOrders.Get(phase.WorkOrderId);
        if (workOrder != null)
        {
            // Reset EndTime if WorkOrder was previously finished (reopen)
            if (workOrder.EndTime.HasValue)
            {
                workOrder.EndTime = null;
            }
            
            // Start WorkOrder if this is the first phase
            if (!workOrder.StartTime.HasValue)
            {
                workOrder.StartTime = DateTime.Now;
                workOrder.StatusId = productionStatus.Id;
            }
            else
            {
                // Ensure status is Production when reopening
                workOrder.StatusId = productionStatus.Id;
            }
            
            unitOfWork.WorkOrders.UpdateWithoutSave(workOrder);
        }
        
        // Persist changes
        await unitOfWork.CompleteAsync();
        
        return new GenericResponse(true, phase);
    }

    public async Task<GenericResponse> EndPhase(Guid phaseId)
    {
        // Get phase
        var phase = await unitOfWork.WorkOrders.Phases.Get(phaseId);
        if (phase == null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));
        
        // Validate phase has been started
        if (!phase.StartTime.HasValue)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotStarted"));
        
        // Update phase
        phase.EndTime = DateTime.Now;
        unitOfWork.WorkOrders.Phases.UpdateWithoutSave(phase);
        
        // Get WorkOrder with all phases to check if this is the last one
        var workOrder = await unitOfWork.WorkOrders.Get(phase.WorkOrderId);
        if (workOrder == null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderNotFound", phase.WorkOrderId));
        
        // Check if this is the last active phase
        var activePhasesOrdered = workOrder.Phases
            .Where(p => !p.Disabled)
            .OrderByDescending(p => p.CodeAsNumber)
            .ToList();
        
        bool isLastPhase = activePhasesOrdered.FirstOrDefault()?.Id == phaseId;
        
        // If last phase, close the work order
        if (isLastPhase)
        {
            var closedStatus = await unitOfWork.Lifecycles.GetStatusByName(
                StatusConstants.Lifecycles.WorkOrder, 
                StatusConstants.Statuses.Tancada);
            
            if (closedStatus == null)
            {
                logger.LogError("Closed status 'Tancada' not found in WorkOrder lifecycle");
            }
            else
            {
                workOrder.StatusId = closedStatus.Id;
                workOrder.EndTime = DateTime.Now;
                unitOfWork.WorkOrders.UpdateWithoutSave(workOrder);
            }
        }
        
        // Persist changes
        await unitOfWork.CompleteAsync();
        
        return new GenericResponse(true, phase);
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

    public async Task<IEnumerable<WorkOrderWithPhasesDto>> GetPlannedByWorkcenterType(
        Guid workcenterTypeId)
    {
        // Get work orders with phases from repository (efficient EF query)
        var (workOrders, statusLookup) = await unitOfWork.WorkOrders
            .GetWorkOrdersWithPlannedPhases(workcenterTypeId);

        // Transform entities to hierarchical DTOs
        var results = new List<WorkOrderWithPhasesDto>();

        foreach (var wo in workOrders)
        {
            var plannedPhases = new List<PlannedPhaseDto>();

            foreach (var phase in wo.Phases)
            {
                plannedPhases.Add(new PlannedPhaseDto
                {
                    PhaseId = phase.Id,
                    PhaseCode = phase.Code,
                    PhaseDescription = phase.Description ?? string.Empty,
                    PhaseDisplay = $"{phase.Code} - {phase.Description}",
                    PhaseStatus = statusLookup.GetValueOrDefault(phase.Id, string.Empty),
                    StartTime = phase.StartTime,
                    PreferredWorkcenterName = phase.PreferredWorkcenter?.Name ?? string.Empty
                });
            }

            if (plannedPhases.Count != 0)
            {
                results.Add(new WorkOrderWithPhasesDto
                {
                    WorkOrderId = wo.Id,
                    WorkOrderCode = wo.Code,
                    CustomerName = wo.Reference?.Customer?.ComercialName ?? string.Empty,
                    SalesReferenceDisplay = wo.Reference != null 
                        ? $"{wo.Reference.Code} - {wo.Reference.Description}"
                        : string.Empty,
                    PlannedQuantity = wo.PlannedQuantity,
                    PlannedDate = wo.PlannedDate,
                    StartTime = wo.StartTime,
                    WorkOrderStatus = wo.Status?.Name ?? string.Empty,
                    Priority = wo.Order,
                    Phases = plannedPhases
                });
            }
        }

        return results;
    }

    public async Task<IEnumerable<WorkOrderWithPhasesDto>> GetLoadedWorkOrdersByPhaseIds(List<Guid> phaseIds)
    {
        if (phaseIds == null || phaseIds.Count == 0)
            return [];

        // Use optimized repository method with efficient EF Core query
        var workOrders = await unitOfWork.WorkOrders.GetWorkOrdersByLoadedPhaseIds(phaseIds);
        
        if (!workOrders.Any())
            return [];

        // Transform to DTOs
        var results = new List<WorkOrderWithPhasesDto>();

        foreach (var wo in workOrders)
        {
            var plannedPhases = new List<PlannedPhaseDto>();

            foreach (var phase in wo.Phases)
            {
                plannedPhases.Add(new PlannedPhaseDto
                {
                    PhaseId = phase.Id,
                    PhaseCode = phase.Code,
                    PhaseDescription = phase.Description ?? string.Empty,
                    PhaseDisplay = $"{phase.Code} - {phase.Description}",
                    PhaseStatus = phase.Status?.Name ?? string.Empty,
                    StartTime = phase.StartTime,
                    PreferredWorkcenterName = phase.PreferredWorkcenter?.Name ?? string.Empty
                });
            }

            results.Add(new WorkOrderWithPhasesDto
            {
                WorkOrderId = wo.Id,
                WorkOrderCode = wo.Code,
                CustomerName = wo.Reference?.Customer?.ComercialName ?? string.Empty,
                SalesReferenceDisplay = wo.Reference != null 
                    ? $"{wo.Reference.Code} - {wo.Reference.Description}"
                    : string.Empty,
                PlannedQuantity = wo.PlannedQuantity,
                PlannedDate = wo.PlannedDate,
                StartTime = wo.StartTime,
                WorkOrderStatus = wo.Status?.Name ?? string.Empty,
                Priority = wo.Order,
                Phases = plannedPhases
            });
        }

        return results;
    }

    public async Task<IEnumerable<WorkOrderPhaseDetailedDto>> GetWorkOrderPhasesDetailed(Guid workOrderId)
    {
        var workOrder = await unitOfWork.WorkOrders.GetWorkOrderWithPhasesDetailed(workOrderId);
        
        if (workOrder == null || workOrder.Phases == null)
            return new List<WorkOrderPhaseDetailedDto>();

        var results = new List<WorkOrderPhaseDetailedDto>();

        // Build sales reference display (Customer + Reference + Version)
        var salesReferenceDisplay = string.Empty;
        if (workOrder.Reference != null)
        {
            var customerName = workOrder.Reference.Customer?.ComercialName ?? string.Empty;
            var referenceCode = workOrder.Reference.Code ?? string.Empty;
            var version = !string.IsNullOrEmpty(workOrder.Reference.Version) 
                ? $" v{workOrder.Reference.Version}" 
                : string.Empty;
            
            salesReferenceDisplay = $"{customerName} - {referenceCode}{version}";
        }

        foreach (var phase in workOrder.Phases.Where(p => !p.Disabled))
        {
            var detailedPhase = new WorkOrderPhaseDetailedDto
            {
                WorkOrderId = workOrder.Id,
                WorkOrderCode = workOrder.Code,
                SalesReferenceDisplay = salesReferenceDisplay,
                PlannedQuantity = workOrder.PlannedQuantity,
                PhaseId = phase.Id,
                PhaseCode = phase.Code,
                PhaseDescription = phase.Description,
                PhaseStatus = phase.Status?.Name ?? string.Empty,
                StartTime = phase.StartTime,
                EndTime = phase.EndTime,
                PreferredWorkcenterName = phase.PreferredWorkcenter?.Name ?? string.Empty,
                WorkcenterTypeId = phase.WorkcenterTypeId ?? Guid.Empty
            };

            // Map phase details
            if (phase.Details != null)
            {
                foreach (var detail in phase.Details.Where(d => !d.Disabled))
                {
                    detailedPhase.Details.Add(new PhaseDetailItemDto
                    {
                        MachineStatusId = detail.MachineStatusId,
                        MachineStatusName = detail.MachineStatus?.Name ?? string.Empty,
                        EstimatedTime = detail.EstimatedTime,
                        EstimatedOperatorTime = detail.EstimatedOperatorTime,
                        IsCycleTime = detail.IsCycleTime,
                        Comment = detail.Comment ?? string.Empty
                    });
                }
            }

            // Map bill of materials
            if (phase.BillOfMaterials != null)
            {
                foreach (var bom in phase.BillOfMaterials.Where(b => !b.Disabled))
                {
                    detailedPhase.BillOfMaterials.Add(new BillOfMaterialsItemDto
                    {
                        ReferenceCode = bom.Reference?.Code ?? string.Empty,
                        ReferenceDescription = bom.Reference?.Description ?? string.Empty,
                        Quantity = bom.Quantity
                    });
                }
            }

            results.Add(detailedPhase);
        }

        return results;
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

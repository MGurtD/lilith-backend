using Application.Contracts;
using Application.Contracts.Production;
using Application.Contracts.Purchase;
using Application.Persistance;
using Application.Production.Warehouse;
using Application.Services;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Production;

[ApiController]
[Route("api/[controller]")]
public class PlanningController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private const string WorkOrderLifecycle = "WorkOrder";

    public PlanningController(IUnitOfWork unitOfWork, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    [HttpGet("Workcenter/{id:guid}")]
    public async Task<IActionResult> GetByWorkcenterId(Guid id)
    {
        // Obtenir centre de treball
        var workcenter = await _unitOfWork.Workcenters.Get(id);
        if (workcenter == null)
            return NotFound(id);

        // Obtener el cicle de vida
        var workOrderLifecycle = await _unitOfWork.Lifecycles.GetByName(WorkOrderLifecycle);
        if (workOrderLifecycle == null || workOrderLifecycle.Statuses == null)
            return NotFound(WorkOrderLifecycle);
        // Obtener el estats planificables
        var validStatuses = new List<string> { "Creada", "Llançada", "Pausa" };
        var validStatusIds = workOrderLifecycle.Statuses.Where(p => validStatuses.Contains(p.Name)).Select(s => s.Id).ToList();

        // Consultar les fases programades en aquest centre de treball
        var workOrdersPhases = _unitOfWork.WorkOrders.Phases
            .Find(p => (p.PreferredWorkcenterId == id || p.WorkcenterTypeId == workcenter.WorkcenterTypeId) && validStatusIds.Contains(p.StatusId))
            .OrderBy(p => p.Code)
            .ToList();
        // Consultar les ordres de treball de les fases
        var workorderIds = workOrdersPhases.GroupBy(p => p.WorkOrderId).Select(g => g.Key).ToList();
        var workorders = _unitOfWork.WorkOrders.Find(wo => workorderIds.Contains(wo.Id));
        // Vincular-les
        foreach (var workOrdersPhase in workOrdersPhases)
            workOrdersPhase.WorkOrder = workorders.Where(p => p.Id == workOrdersPhase.WorkOrderId).FirstOrDefault();

        var orderedWorkOrderPhases = workOrdersPhases.OrderBy(p => p.WorkOrder!.PlannedDate).OrderBy(p => p.WorkOrder!.Order);
        return Ok(orderedWorkOrderPhases);
    }

    [HttpGet("WorkOrderPhase/{id:guid}")]
    public async Task<IActionResult> GetByWorkOrderPhaseId(Guid id)
    {
        // Obtenir fase de treball
        var workOrderPhase = await _unitOfWork.WorkOrders.Phases.Get(id);
        if (workOrderPhase == null)
            return NotFound(new GenericResponse(false, $"Fase de ordre de fabricació amb ID {id}"));

        // Obtenir ordre de treball
        var workOrder = await _unitOfWork.WorkOrders.Get(workOrderPhase.WorkOrderId);
        workOrderPhase.WorkOrder = workOrder;
        if (workOrder == null)
            return NotFound(new GenericResponse(false, $"Ordre de fabricación amb ID {workOrderPhase.WorkOrderId}"));

        var referencePdfs = _fileService.GetEntityFiles("referenceMaps", workOrder.ReferenceId).Where(f => f.Path.EndsWith("pdf"));

        return Ok(new {
            workOrderPhase,
            referencePdfs
        });
    }
}

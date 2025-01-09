using Application.Contracts.Production;
using Application.Persistance;
using Application.Services.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production;

[ApiController]
[Route("api/[controller]")]
public class WorkcenterShiftController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkcenterShiftService _workcenterShiftService;

    public WorkcenterShiftController(IUnitOfWork unitOfWork, IWorkcenterShiftService workcenterShiftService)
    {
        _unitOfWork = unitOfWork;
        _workcenterShiftService = workcenterShiftService;
    }

    [HttpGet("{workcenterShiftId:guid}")]
    public async Task<IActionResult> GetWorkcenterShift(Guid workcenterShiftId)
    {
        var workcenterShift = await _workcenterShiftService.GetWorkcenterShift(workcenterShiftId);
        if (workcenterShift is not null)
        {
            return Ok(workcenterShift);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("Current")]
    public async Task<IActionResult> GetCurrentWorkcenterShifts()
    {
        var workcenterShifts = await _workcenterShiftService.GetCurrentWorkcenterShifts();
        return Ok(workcenterShifts);
    }

    [HttpPost("CreateWorkcenterShifts")]
    public async Task<IActionResult> CreateWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos)
    {
        var response = await _workcenterShiftService.CreateWorkcenterShifts(dtos);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPost("Operator/In")]
    public async Task<IActionResult> OperatorIn(OperatorInOutRequest request)
    {
        var response = await _workcenterShiftService.OperatorIn(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }
    [HttpPost("Operator/Out")]
    public async Task<IActionResult> OperatorOut(OperatorInOutRequest request)
    {
        var response = await _workcenterShiftService.OperatorOut(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPost("Phase/In")]
    public async Task<IActionResult> WorkOrderPhaseIn(WorkOrderPhaseInOutRequest request)
    {
        var response = await _workcenterShiftService.WorkOrderPhaseIn(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }
    [HttpPost("Phase/Out")]
    public async Task<IActionResult> WorkOrderPhaseOut(WorkOrderPhaseInOutRequest request)
    {
        var response = await _workcenterShiftService.WorkOrderPhaseOut(request);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPut("UpdateWorkcenterShiftDetailQuantities")]
    public async Task<IActionResult> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto)
    {
        var response = await _workcenterShiftService.UpdateWorkcenterShiftDetailQuantities(dto);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPut("DisableWorkcenterShift/{workcenterShiftId:guid}")]
    public async Task<IActionResult> DisableWorkcenterShift(Guid workcenterShiftId)
    {
        var response = await _workcenterShiftService.DisableWorkcenterShift(workcenterShiftId);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpPut("DisableWorkcenterShiftDetail/{workcenterShiftDetailId:guid}")]
    public async Task<IActionResult> DisableWorkcenterShiftDetail(Guid workcenterShiftDetailId)
    {
        var response = await _workcenterShiftService.DisableWorkcenterShiftDetail(workcenterShiftDetailId);
        if (response.Result)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }

}
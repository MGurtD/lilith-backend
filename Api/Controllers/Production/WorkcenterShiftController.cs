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

    [HttpGet("Workcenter/{workcenterId:guid}")]
    public async Task<IActionResult> GetWorkcenterShifts(Guid workcenterId)
    {
        var workcenterShifts = await _workcenterShiftService.GetWorkcenterShifts(workcenterId);
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

    [HttpPost("CreateWorkcenterShiftDetail")]
    public async Task<IActionResult> CreateWorkcenterShiftDetail(CreateWorkcenterShiftDetailDto dto)
    {
        var response = await _workcenterShiftService.CreateWorkcenterShiftDetail(dto);
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
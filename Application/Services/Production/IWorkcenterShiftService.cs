using Application.Contracts;
using Application.Contracts.Production;
using Domain.Entities.Production;

namespace Application.Services.Production;

public interface IWorkcenterShiftService
{
    Task<WorkcenterShift?> GetWorkcenterShift(Guid workcenterShiftId);
    Task<List<WorkcenterShift>> GetWorkcenterShifts(Guid workcenterShiftId);

    Task<GenericResponse> CreateWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos);
    
    Task<GenericResponse> CreateWorkcenterShiftDetail(CreateWorkcenterShiftDetailDto dto);
    Task<GenericResponse> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto);

    Task<GenericResponse> DisableWorkcenterShift(Guid workcenterShiftId);
    Task<GenericResponse> DisableWorkcenterShiftDetail(Guid workcenterShiftDetailId);
}

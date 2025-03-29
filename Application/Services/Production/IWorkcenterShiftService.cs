using Application.Contracts;
using Application.Contracts.Production;
using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;

namespace Application.Services.Production;

public interface IWorkcenterShiftService
{
    IWorkcenterShiftDetailService DetailsService { get; }

    Task<WorkcenterShift?> GetWorkcenterShift(Guid workcenterShiftId);
    Task<List<WorkcenterShift>> GetCurrentWorkcenterShifts();

    Task<GenericResponse> CreateWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos);

    Task<GenericResponse> DisableWorkcenterShift(Guid workcenterShiftId);
}

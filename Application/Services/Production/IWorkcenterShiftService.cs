using Application.Contracts;
using Application.Contracts.Production;
using Domain.Entities.Production;

namespace Application.Services.Production;

public interface IWorkcenterShiftService
{
    Task<WorkcenterShift?> GetWorkcenterShift(Guid workcenterShiftId);
    Task<List<WorkcenterShift>> GetCurrentWorkcenterShifts();

    Task<GenericResponse> CreateWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos);

    Task<GenericResponse> OperatorIn(OperatorInOutRequest request);
    Task<GenericResponse> OperatorOut(OperatorInOutRequest request);
    Task<GenericResponse> WorkOrderPhaseIn(WorkOrderPhaseInOutRequest request);
    Task<GenericResponse> WorkOrderPhaseOut(WorkOrderPhaseInOutRequest request);
    Task<GenericResponse> ChangeWorkcenterStatus(WorkcenterChangeStatusRequest request);
    Task<GenericResponse> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto);

    Task<GenericResponse> DisableWorkcenterShift(Guid workcenterShiftId);
    Task<GenericResponse> DisableWorkcenterShiftDetail(Guid workcenterShiftDetailId);
}

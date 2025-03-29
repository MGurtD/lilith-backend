﻿using Application.Contracts;
using Application.Contracts.Production;
using Domain.Entities.Production;

namespace Application.Services.Production;

public interface IWorkcenterShiftDetailService
{
    Task<WorkcenterShiftDetail?> GetWorkcenterShiftDetailById(Guid id);
    Task<List<WorkcenterShiftDetail>> GetCurrentWorkcenterShiftDetailsByWorkcenterShiftId(Guid workcenterShiftDetailId);

    Task<GenericResponse> OperatorIn(OperatorInOutRequest request);
    Task<GenericResponse> OperatorOut(OperatorInOutRequest request);
    Task<GenericResponse> WorkOrderPhaseIn(WorkOrderPhaseInOutRequest request);
    Task<GenericResponse> WorkOrderPhaseOut(WorkOrderPhaseInOutRequest request);
    Task<GenericResponse> ChangeWorkcenterStatus(WorkcenterChangeStatusRequest request);
    Task<GenericResponse> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto);

    Task<GenericResponse> DisableWorkcenterShiftDetail(Guid workcenterShiftDetailId);
}

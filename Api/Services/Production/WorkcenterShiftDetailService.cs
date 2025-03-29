using Application.Contracts;
using Application.Contracts.Production;
using Application.Persistance;
using Application.Services.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Production
{
    public class WorkcenterShiftDetailService(IUnitOfWork unitOfWork) : IWorkcenterShiftDetailService
    {

        public async Task<WorkcenterShiftDetail?> GetWorkcenterShiftDetailById(Guid id)
        {
            return await unitOfWork.WorkcenterShifts.Details.Get(id);
        }

        public async Task<List<WorkcenterShiftDetail>> GetCurrentWorkcenterShiftDetailsByWorkcenterShiftId(Guid workcenterShiftDetailId)
        {
            var currentWorkcenterShift = await unitOfWork.WorkcenterShifts.Get(workcenterShiftDetailId);
            if (currentWorkcenterShift == null)
            {
                return [];
            }

            return currentWorkcenterShift.Details
                .Where(wsd => wsd.Current)
                .ToList();
        }

        #region ClockInOutOperator
        private static GenericResponse ValidateOperatorClockInOut(List<WorkcenterShiftDetail> currentWorkcenterDetails, Guid operatorId, OperatorDirection direction)
        {
            if (direction == OperatorDirection.In && currentWorkcenterDetails.Any(wsd => wsd.OperatorId == operatorId))
            {
                return new GenericResponse(false, "L'operari indicat es troba actualment al centre de treball");

            }
            else if (direction == OperatorDirection.Out && !currentWorkcenterDetails.Any(wsd => wsd.OperatorId == operatorId))
            {
                return new GenericResponse(false, "L'operari indicat no es troba actualment al centre de treball");
            }
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> OperatorIn(OperatorInOutRequest request)
        {
            var currentWorkcenterShifts = await unitOfWork.WorkcenterShifts
                                                    .FindWithDetails(ws => ws.Current)
                                                    .ToListAsync();

            var currentDetailsWithOperator = currentWorkcenterShifts
                                                    .SelectMany(ws => ws.Details.Where(wsd => wsd.Current && wsd.OperatorId == request.OperatorId))
                                                    .ToList();

            var currentWorkcenterShift = currentWorkcenterShifts.First(ws => ws.WorkcenterId == request.WorkcenterId);
            var currentWorkcenterDetails = currentWorkcenterShift.Details
                                                    .Where(wsd => wsd.Current)
                                                    .ToList();

            var validationResponse = ValidateOperatorClockInOut(currentWorkcenterDetails, request.OperatorId, OperatorDirection.In);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            if (currentWorkcenterDetails.Count == 0)
            {
                // Cas 1 : No hi ha detalls actius al centre de treball
                var defaultMachineStatus = (await unitOfWork.MachineStatuses.FindAsync(ms => ms.Default)).FirstOrDefault();
                if (defaultMachineStatus == null) return new GenericResponse(false, "L'estat per defecte no existeix");

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterShift.Id,
                    MachineStatusId = defaultMachineStatus.Id,
                    OperatorId = request.OperatorId,
                    ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count + 1
                };
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }
            else
            {
                // Cas 2 : Hi ha detalls actius al centre de treball
                foreach (var currentWorkcenterDetail in currentWorkcenterDetails)
                {
                    if (currentWorkcenterDetail.OperatorId == null)
                    {
                        currentWorkcenterDetail.Close(request.Timestamp);
                        unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentWorkcenterDetail);
                    }

                    var newDetail = new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = currentWorkcenterDetail.WorkcenterShiftId,
                        MachineStatusId = currentWorkcenterDetail.MachineStatusId,
                        WorkOrderPhaseId = currentWorkcenterDetail.WorkOrderPhaseId,
                        OperatorId = request.OperatorId,
                        ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count + 1
                    };
                    newDetail.Open(request.Timestamp);

                    await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
                }
            }

            // Cas 3 : Hi ha detalls actius a altres centres de treball. Cal tancar i obrir nous detalls amb un operari concurrent més
            foreach (var otherWorkcenterDetail in currentDetailsWithOperator.Where(cdo => cdo.WorkcenterShiftId != currentWorkcenterShift.Id))
            {
                otherWorkcenterDetail.Close(request.Timestamp);
                unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(otherWorkcenterDetail);

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = otherWorkcenterDetail.WorkcenterShiftId,
                    MachineStatusId = otherWorkcenterDetail.MachineStatusId,
                    WorkOrderPhaseId = otherWorkcenterDetail.WorkOrderPhaseId,
                    OperatorId = request.OperatorId,
                    ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count + 1
                };
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            await unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> OperatorOut(OperatorInOutRequest request)
        {
            var currentWorkcenterShifts = await unitOfWork.WorkcenterShifts
                                                    .FindWithDetails(ws => ws.Current)
                                                    .ToListAsync();

            var currentDetailsWithOperator = currentWorkcenterShifts
                                                    .SelectMany(ws => ws.Details.Where(wsd => wsd.Current && wsd.OperatorId == request.OperatorId))
                                                    .ToList();

            var currentWorkcenterShift = currentWorkcenterShifts.First(ws => ws.WorkcenterId == request.WorkcenterId);
            var currentWorkcenterDetails = currentWorkcenterShift.Details
                                                    .Where(wsd => wsd.Current && wsd.OperatorId == request.OperatorId)
                                                    .ToList();

            var validationResponse = ValidateOperatorClockInOut(currentWorkcenterDetails, request.OperatorId, OperatorDirection.Out);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            foreach (var currentWorkcenterDetail in currentDetailsWithOperator)
            {
                currentWorkcenterDetail.Close(request.Timestamp);
                unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentWorkcenterDetail);

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterDetail.WorkcenterShiftId,
                    MachineStatusId = currentWorkcenterDetail.MachineStatusId,
                    WorkOrderPhaseId = currentWorkcenterDetail.WorkOrderPhaseId
                };
                if (currentWorkcenterShift.Id == currentWorkcenterDetail.WorkcenterShiftId)
                {
                    newDetail.OperatorId = null;
                    newDetail.ConcurrentOperatorWorkcenters = 0;
                }
                else
                {
                    newDetail.OperatorId = currentWorkcenterDetail.OperatorId;
                    newDetail.ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count - 1;
                }
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            await unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }
        #endregion

        #region WorkOrderPhaseInOut
        private static GenericResponse ValidateWorkOrderPhase(List<WorkcenterShiftDetail> currentWorkcenterDetails, WorkOrderPhaseInOutRequest request, OperatorDirection direction)
        {
            if (direction == OperatorDirection.In)
            {
                if (currentWorkcenterDetails.Any(wsd => wsd.WorkOrderPhaseId == request.WorkOrderPhaseId))
                {
                    return new GenericResponse(false, "La fase de fabricació indicada es troba actualment al centre de treball");
                }

                // TODO : Validar número de fases concurrents en el centre de treball (pendent crear camp i implementar validació)
            }
            else if (direction == OperatorDirection.Out && !currentWorkcenterDetails.Any(wsd => wsd.WorkOrderPhaseId == request.WorkOrderPhaseId))
            {
                return new GenericResponse(false, "La fase de fabricació indicada no es troba actualment al centre de treball");
            }
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> WorkOrderPhaseIn(WorkOrderPhaseInOutRequest request)
        {
            var currentWorkcenterShift = await unitOfWork.WorkcenterShifts.GetCurrentWorkcenterShiftWithCurrentDetails(request.WorkcenterId);
            if (currentWorkcenterShift == null)
            {
                return new GenericResponse(false, "El centre de treball no té cap torn actiu");
            }
            var currentWorkcenterShiftDetails = currentWorkcenterShift.Details.ToList();

            // Validar la petició contra l'estat del sistema
            var validationResponse = ValidateWorkOrderPhase(currentWorkcenterShiftDetails, request, OperatorDirection.In);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            if (currentWorkcenterShiftDetails.Count == 0)
            {
                // No hi ha detalls actius al centre de treball, crear un nou detall amb l'estat per defecte
                var defaultMachineStatus = (await unitOfWork.MachineStatuses.FindAsync(ms => ms.Default)).FirstOrDefault();
                if (defaultMachineStatus == null) return new GenericResponse(false, "L'estat per defecte no existeix");
                
                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterShift.Id,
                    MachineStatusId = defaultMachineStatus.Id,
                    WorkOrderPhaseId = request.WorkOrderPhaseId
                };
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }
            else
            {
                foreach (var currentDetail in currentWorkcenterShiftDetails.Where(wsd => wsd.WorkOrderPhaseId == null))
                {
                    currentDetail.Close(request.Timestamp);
                    unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentDetail);

                    var newDetail = new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = currentDetail.WorkcenterShiftId,
                        MachineStatusId = currentDetail.MachineStatusId,
                        OperatorId = currentDetail.OperatorId,
                        WorkOrderPhaseId = request.WorkOrderPhaseId,
                        ConcurrentOperatorWorkcenters = currentDetail.ConcurrentOperatorWorkcenters
                    };
                    newDetail.Open(request.Timestamp);
                    await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
                }
            }

            await unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> WorkOrderPhaseOut(WorkOrderPhaseInOutRequest request)
        {
            var currentWorkcenterShift = await unitOfWork.WorkcenterShifts.GetCurrentWorkcenterShiftWithCurrentDetails(request.WorkcenterId);
            if (currentWorkcenterShift == null)
            {
                return new GenericResponse(false, "El centre de treball no té cap torn actiu");
            }
            var currentWorkcenterShiftDetails = currentWorkcenterShift.Details.ToList();

            // Validar petició
            var validationResponse = ValidateWorkOrderPhase(currentWorkcenterShiftDetails, request, OperatorDirection.Out);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            // Tancar la fase de fabricació al centre de treball
            foreach (var currentDetail in currentWorkcenterShiftDetails.Where(wsd => wsd.WorkOrderPhaseId == request.WorkOrderPhaseId))
            {
                currentDetail.Close(request.Timestamp);
                unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentDetail);

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentDetail.WorkcenterShiftId,
                    MachineStatusId = currentDetail.MachineStatusId,
                    OperatorId = currentDetail.OperatorId,
                    ConcurrentOperatorWorkcenters = currentDetail.ConcurrentOperatorWorkcenters,
                    WorkOrderPhaseId = null
                };
                newDetail.Open(request.Timestamp);
                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            await unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }
        #endregion

        #region ChangeWorkcenterStatus

        public async Task<GenericResponse> ChangeWorkcenterStatus(WorkcenterChangeStatusRequest request)
        {
            // Obtenir el torn actual del centre de treball
            var currentWorkcenterShift = await unitOfWork.WorkcenterShifts
                                                .FindWithDetails(ws => ws.Current && ws.WorkcenterId == request.WorkcenterId)
                                                .FirstOrDefaultAsync();
            if (currentWorkcenterShift == null)
            {
                return new GenericResponse(false, "El centre de treball no té cap torn actiu");
            }

            // Obtenir els detalls actius del centre de treball
            var currentDetails = currentWorkcenterShift.Details
                                                .Where(wsd => wsd.Current)
                                                .ToList();

            // Validar la petició
            var validationResponse = await ValidateWorkcenterStatus(currentDetails, request.MachineStatusId);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            if (currentDetails.Count == 0)
            {
                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterShift.Id,
                    MachineStatusId = request.MachineStatusId
                };
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }
            else
            {
                foreach (var detail in currentDetails)
                {
                    detail.Close(request.Timestamp);
                    unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(detail);

                    var newDetail = new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = detail.WorkcenterShiftId,
                        MachineStatusId = request.MachineStatusId,
                        OperatorId = detail.OperatorId,
                        WorkOrderPhaseId = detail.WorkOrderPhaseId,
                        ConcurrentOperatorWorkcenters = detail.ConcurrentOperatorWorkcenters
                    };
                    newDetail.Open(request.Timestamp);

                    await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
                }
            }

            await unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        private async Task<GenericResponse> ValidateWorkcenterStatus(List<WorkcenterShiftDetail> currentDetails, Guid machineStatusId)
        {
            var machineStatus = await unitOfWork.MachineStatuses.Get(machineStatusId);
            if (machineStatus == null)
            {
                return new GenericResponse(false, "L'estat indicat no existeix");
            }
            if (currentDetails.Any(wsd => wsd.MachineStatusId == machineStatusId))
            {
                return new GenericResponse(false, "El centre de treball ja té l'estat indicat");
            }
            return new GenericResponse(true);
        }
        #endregion

        public async Task<GenericResponse> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto)
        {
            // Obtener el último detalle de la fase de la orden de trabajo
            var recentDetail = await unitOfWork.WorkcenterShifts
                                                .FindWithDetails(ws => ws.Current && ws.WorkcenterId == dto.WorkcenterId)
                                                .SelectMany(ws => ws.Details
                                                    .Where(wsd => wsd.Current && wsd.WorkOrderPhaseId == dto.WorkOrderPhaseId))
                                                .OrderByDescending(wsd => wsd.StartTime)
                                                .FirstOrDefaultAsync();
            if (recentDetail != null)
            {
                recentDetail.QuantityOk += dto.QuantityOk;
                recentDetail.QuantityKo += dto.QuantityKo;
                await unitOfWork.WorkcenterShifts.Details.Update(recentDetail);
                return new GenericResponse(true);
            }
            else
            {
                return new GenericResponse(false, "La fase de fabricació no está carregada al centre de treball indicat");
            }
        }

        public async Task<GenericResponse> DisableWorkcenterShiftDetail(Guid workcenterShiftDetailId)
        {
            var workcenterShiftDetail = await unitOfWork.WorkcenterShifts.Details.Get(workcenterShiftDetailId);
            if (workcenterShiftDetail == null)
            {
                return new GenericResponse(false, "Workcenter shift detail not found");
            }

            workcenterShiftDetail.Disabled = true;
            await unitOfWork.WorkcenterShifts.Details.Update(workcenterShiftDetail);
            return new GenericResponse(true);
        }


    }
}

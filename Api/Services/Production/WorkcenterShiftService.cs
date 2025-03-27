using Application.Contracts;
using Application.Contracts.Production;
using Application.Persistance;
using Application.Services.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Api.Services.Production
{
    public class WorkcenterShiftService : IWorkcenterShiftService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkcenterShiftService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WorkcenterShift?> GetWorkcenterShift(Guid workcenterShiftId)
        {
            var workcenterShift = await _unitOfWork.WorkcenterShifts.Get(workcenterShiftId);
            return workcenterShift;
        }

        public async Task<List<WorkcenterShift>> GetCurrentWorkcenterShifts()
        {
            var workcenterShifts = await _unitOfWork.WorkcenterShifts.FindWithDetails(wsd => wsd.Current).ToListAsync();
            return [.. workcenterShifts];
        }

        public async Task<GenericResponse> CreateWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos)
        {
            // Obtenir els torns actius dels centres de treball indicats en els DTOs
            var currentWorkcenterShifts = await _unitOfWork.WorkcenterShifts
                .FindWithDetails(ws => dtos.Select(dto => dto.WorkcenterId).Contains(ws.WorkcenterId) && ws.Current)
                .ToListAsync();

            var newWorkcenterShifts = CreateUnexistingWorkcenterShifts(dtos, currentWorkcenterShifts);
            var newWorkcenterShiftDetails = new List<WorkcenterShiftDetail>();

            foreach (var workcenterShift in currentWorkcenterShifts)
            {
                var dto = dtos.First(dto => dto.WorkcenterId == workcenterShift.WorkcenterId)!;

                workcenterShift.Current = false;
                workcenterShift.EndTime = dto.StartTime;
                _unitOfWork.WorkcenterShifts.UpdateWithoutSave(workcenterShift);

                var newWorkcenterShift = new WorkcenterShift()
                {
                    WorkcenterId = dto.WorkcenterId,
                    ShiftDetailId = dto.ShiftDetailId,
                    Current = true,
                    StartTime = dto.StartTime,
                    EndTime = null
                };
                newWorkcenterShifts.Add(newWorkcenterShift);

                foreach (var detail in workcenterShift.Details)
                {
                    detail.Current = false;
                    detail.EndTime = dto.StartTime;
                    _unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(detail);

                    newWorkcenterShiftDetails.Add(new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = newWorkcenterShift.Id,
                        MachineStatusId = detail.MachineStatusId,
                        OperatorId = detail.OperatorId,
                        WorkOrderPhaseId = detail.WorkOrderPhaseId,
                        StartTime = dto.StartTime,
                        Current = true,
                        EndTime = null
                    });
                }
            }

            await _unitOfWork.WorkcenterShifts.AddRangeWithoutSave(newWorkcenterShifts);
            await _unitOfWork.WorkcenterShifts.Details.AddRangeWithoutSave(newWorkcenterShiftDetails);

            await _unitOfWork.CompleteAsync();

            return new GenericResponse(true);
        }

        public List<WorkcenterShift> CreateUnexistingWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos, List<WorkcenterShift> currentWorkcenterShifts)
        {
            var newWorkcenterShifts = new List<WorkcenterShift>();

            // Obtenim els WorkcenterIds dels torns actius
            var currentWorkcenterIds = currentWorkcenterShifts
                .Select(ws => ws.WorkcenterId)
                .Distinct()
                .ToList();

            // Identifiquem els DTOs que no tenen un torn actiu
            var newWorkcenterDtos = dtos
                .Where(dto => !currentWorkcenterIds.Contains(dto.WorkcenterId))
                .ToList();

            // Creem els nous torns i els seus detalls
            foreach (var dto in newWorkcenterDtos)
            {
                var workcenterShift = new WorkcenterShift()
                {
                    WorkcenterId = dto.WorkcenterId,
                    ShiftDetailId = dto.ShiftDetailId,
                    StartTime = dto.StartTime,
                    EndTime = null
                };
                newWorkcenterShifts.Add(workcenterShift);
            }
            return newWorkcenterShifts;
        }

        #region ClockInOutOperator
        public async Task<GenericResponse> OperatorIn(OperatorInOutRequest request)
        {
            var currentWorkcenterShifts = await _unitOfWork.WorkcenterShifts
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
                var defaultMachineStatus = (await _unitOfWork.MachineStatuses.FindAsync(ms => ms.Default)).FirstOrDefault();
                if (defaultMachineStatus == null) return new GenericResponse(false, "L'estat per defecte no existeix");

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterShift.Id,
                    MachineStatusId = defaultMachineStatus.Id,
                    OperatorId = request.OperatorId,
                    ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count + 1
                };
                newDetail.Open(request.Timestamp);

                await _unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }
            else
            {
                // Cas 2 : Hi ha detalls actius al centre de treball
                foreach (var currentWorkcenterDetail in currentWorkcenterDetails)
                {
                    if (currentWorkcenterDetail.OperatorId == null)
                    {
                        currentWorkcenterDetail.Close(request.Timestamp);
                        _unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentWorkcenterDetail);
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

                    await _unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
                }
            }

            // Cas 3 : Hi ha detalls actius a altres centres de treball. Cal tancar i obrir nous detalls amb un operari concurrent més
            foreach (var otherWorkcenterDetail in currentDetailsWithOperator.Where(cdo => cdo.WorkcenterShiftId != currentWorkcenterShift.Id))
            {
                otherWorkcenterDetail.Close(request.Timestamp);
                _unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(otherWorkcenterDetail);

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = otherWorkcenterDetail.WorkcenterShiftId,
                    MachineStatusId = otherWorkcenterDetail.MachineStatusId,
                    WorkOrderPhaseId = otherWorkcenterDetail.WorkOrderPhaseId,
                    OperatorId = request.OperatorId,
                    ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count + 1
                };
                newDetail.Open(request.Timestamp);

                await _unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            await _unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> OperatorOut(OperatorInOutRequest request)
        {
            var currentWorkcenterShifts = await _unitOfWork.WorkcenterShifts
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
                _unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentWorkcenterDetail);

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

                await _unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            await _unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        private GenericResponse ValidateOperatorClockInOut(List<WorkcenterShiftDetail> currentWorkcenterDetails, Guid operatorId, OperatorDirection direction)
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
        #endregion

        #region WorkOrderPhaseInOut
        private GenericResponse ValidateWorkOrderPhase(List<WorkcenterShiftDetail> currentWorkcenterDetails, Guid workOrderPhaseId, OperatorDirection direction)
        {
            if (direction == OperatorDirection.In && currentWorkcenterDetails.Any(wsd => wsd.WorkOrderPhaseId == workOrderPhaseId))
            {
                return new GenericResponse(false, "La fase de fabricació indicada es troba actualment al centre de treball");

            }
            else if (direction == OperatorDirection.Out && !currentWorkcenterDetails.Any(wsd => wsd.WorkOrderPhaseId == workOrderPhaseId))
            {
                return new GenericResponse(false, "La fase de fabricació indicada no es troba actualment al centre de treball");
            }
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> WorkOrderPhaseIn(WorkOrderPhaseInOutRequest request)
        {
            throw new NotImplementedException();

            // Obtenir el torn i el detall actual del centre de treball
            var currentDetails = await _unitOfWork.WorkcenterShifts
                                                .FindWithDetails(ws => ws.Current && ws.WorkcenterId == request.WorkcenterId)
                                                .SelectMany(ws => ws.Details.Where(wsd => wsd.Current))
                                                .ToListAsync();

            // Comprovar si la fase está carregada
            var validationResponse = ValidateWorkOrderPhase(currentDetails, request.WorkOrderPhaseId, OperatorDirection.In);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            /*
            var newDetail = new WorkcenterShiftDetail()
            {
                WorkcenterShiftId = currentWorkcenterShifts.First(ws => ws.WorkcenterId == request.WorkcenterId).Id,
                MachineStatusId = defaultMachineStatus.Id,
                OperatorId = request.OperatorId,
                StartTime = request.Timestamp,
                ConcurrentOperatorWorkcenters = currentOtherWorkcenterDetailsWithOperator.Count + 1,
                Current = true
            };
            await _unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);*/
        }

        public Task<GenericResponse> WorkOrderPhaseOut(WorkOrderPhaseInOutRequest request)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region ChangeWorkcenterStatus

        public async Task<GenericResponse> ChangeWorkcenterStatus(WorkcenterChangeStatusRequest request)
        {
            // Obtenir el torn actual del centre de treball
            var currentWorkcenterShift = await _unitOfWork.WorkcenterShifts
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

                await _unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }
            else
            {
                foreach (var detail in currentDetails)
                {
                    detail.Close(request.Timestamp);
                    _unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(detail);

                    var newDetail = new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = detail.WorkcenterShiftId,
                        MachineStatusId = request.MachineStatusId,
                        OperatorId = detail.OperatorId,
                        WorkOrderPhaseId = detail.WorkOrderPhaseId,
                        ConcurrentOperatorWorkcenters = detail.ConcurrentOperatorWorkcenters
                    };
                    newDetail.Open(request.Timestamp);

                    await _unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
                }
            }

            await _unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        private async Task<GenericResponse> ValidateWorkcenterStatus(List<WorkcenterShiftDetail> currentDetails, Guid machineStatusId)
        {
            var machineStatus = await _unitOfWork.MachineStatuses.Get(machineStatusId);
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


        // WorkOrderPhaseIn > El mateix que els operaris però sense la concurrencia
        // WorkOrderPhaseOut > El mateix que els operaris però sense la concurrencia

        public async Task<GenericResponse> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto)
        {
            // Obtener el último detalle de la fase de la orden de trabajo
            var recentDetail = await _unitOfWork.WorkcenterShifts
                                                .FindWithDetails(ws => ws.Current && ws.WorkcenterId == dto.WorkcenterId)
                                                .SelectMany(ws => ws.Details
                                                    .Where(wsd => wsd.Current && wsd.WorkOrderPhaseId == dto.WorkOrderPhaseId))
                                                .OrderByDescending(wsd => wsd.StartTime)
                                                .FirstOrDefaultAsync();
            if (recentDetail != null)
            {
                recentDetail.QuantityOk += dto.QuantityOk;
                recentDetail.QuantityKo += dto.QuantityKo;
                await _unitOfWork.WorkcenterShifts.Details.Update(recentDetail);
                return new GenericResponse(true);
            }
            else
            {
                return new GenericResponse(false, "La fase de fabricació no está carregada al centre de treball indicat");
            }
        }

        public async Task<GenericResponse> DisableWorkcenterShift(Guid workcenterShiftId)
        {
            var workcenterShift = await _unitOfWork.WorkcenterShifts.Get(workcenterShiftId);
            if (workcenterShift == null)
            {
                return new GenericResponse(false, "Workcenter shift not found");
            }
            workcenterShift.Disabled = true;
            _unitOfWork.WorkcenterShifts.UpdateWithoutSave(workcenterShift);

            var workcenterShiftDetails = await _unitOfWork.WorkcenterShifts.Details.FindAsync(wsd => wsd.WorkcenterShiftId == workcenterShiftId);
            foreach (var workcenterShiftDetail in workcenterShiftDetails)
            {
                workcenterShiftDetail.Disabled = true;
                _unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(workcenterShiftDetail);
            }

            await _unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> DisableWorkcenterShiftDetail(Guid workcenterShiftDetailId)
        {
            var workcenterShiftDetail = await _unitOfWork.WorkcenterShifts.Details.Get(workcenterShiftDetailId);
            if (workcenterShiftDetail == null)
            {
                return new GenericResponse(false, "Workcenter shift detail not found");
            }

            workcenterShiftDetail.Disabled = true;
            await _unitOfWork.WorkcenterShifts.Details.Update(workcenterShiftDetail);
            return new GenericResponse(true);
        }

    }
}

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
        private async Task<GenericResponse> ClockInOutOperator(OperatorInOutRequest request, OperatorDirection direction)
        {
            var currentWorkcenterShifts = await _unitOfWork.WorkcenterShifts
                                                    .FindWithDetails(ws => ws.Current)
                                                    .ToListAsync();

            var currentOtherWorkcenterDetailsWithOperator = currentWorkcenterShifts
                                                    .Where(ws => ws.WorkcenterId != request.WorkcenterId)
                                                    .SelectMany(ws => ws.Details.Where(wsd => wsd.OperatorId == request.OperatorId))
                                                    .ToList();

            var currentWorkcenterDetails = currentWorkcenterShifts
                                                    .Where(wsd => wsd.WorkcenterId == request.WorkcenterId)
                                                    .SelectMany(ws => ws.Details)
                                                    .ToList();

            var detailsToClose = currentOtherWorkcenterDetailsWithOperator.Concat(currentWorkcenterDetails).ToList();

            foreach (var currentDetail in detailsToClose)
            {
                currentDetail.Current = false;
                currentDetail.EndTime = request.Timestamp;
                _unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentDetail);

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentDetail.WorkcenterShiftId,
                    MachineStatusId = currentDetail.MachineStatusId,
                    OperatorId = (direction == OperatorDirection.In ? request.OperatorId : null),
                    StartTime = request.Timestamp,
                    ConcurrentOperatorWorkcenters = currentDetail.ConcurrentOperatorWorkcenters + (direction == OperatorDirection.In ? 1 : -1),
                    Current = true
                };
                await _unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            if (direction == OperatorDirection.In && currentWorkcenterDetails.Count == 0)
            {
                var defaultMachineStatus = (await _unitOfWork.MachineStatuses.FindAsync(ms => ms.Name == "Parada")).FirstOrDefault();
                if (defaultMachineStatus == null) return new GenericResponse(false, "L'estat per defecte no existeix");

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterShifts.First(ws => ws.WorkcenterId == request.WorkcenterId).Id,
                    MachineStatusId = defaultMachineStatus.Id,
                    OperatorId = request.OperatorId,
                    StartTime = request.Timestamp,
                    ConcurrentOperatorWorkcenters = currentOtherWorkcenterDetailsWithOperator.Count + 1,
                    Current = true
                };
                await _unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            await _unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> OperatorIn(OperatorInOutRequest request)
        {
            return await ClockInOutOperator(request, OperatorDirection.In);
        }
        public async Task<GenericResponse> OperatorOut(OperatorInOutRequest request)
        {
            return await ClockInOutOperator(request, OperatorDirection.Out);
        }
        #endregion

        #region WorkOrderPhaseInOut
        public Task<GenericResponse> WorkOrderPhaseIn(WorkOrderPhaseInOutRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GenericResponse> WorkOrderPhaseOut(WorkOrderPhaseInOutRequest request)
        {
            throw new NotImplementedException();
        }
        #endregion

        public Task<GenericResponse> ChangeWorkcenterStatus(WorkcenterChangeStatusRequest request)
        {
            throw new NotImplementedException();
        }

        // ChangeStatus > WorkcenterId, StatusId, Timestamp
        // - Tallar tot l'ho actiu al CT > Crear nous registres amb timestamp DTO

        // WorkOrderPhaseIn > El mateix que els operaris però sense la concurrencia
        // WorkOrderPhaseOut > El mateix que els operaris però sense la concurrencia

        public async Task<GenericResponse> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto)
        {
            // Obtener el último detalle de la fase de la orden de trabajo
            var recentDetail = await _unitOfWork.WorkcenterShifts
                                                .FindWithDetails(ws => ws.WorkcenterId == dto.WorkcenterId)
                                                .SelectMany(ws => ws.Details
                                                    .Where(wsd => wsd.WorkOrderPhaseId == dto.WorkOrderPhaseId && wsd.Current))
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
                return new GenericResponse(false, "No open workcenter shift detail");
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

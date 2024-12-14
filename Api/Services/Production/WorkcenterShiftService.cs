using Application.Contracts;
using Application.Contracts.Production;
using Application.Persistance;
using Application.Services.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<WorkcenterShift>> GetWorkcenterShifts(Guid workcenterId)
        {
            var workcenterShifts = await _unitOfWork.WorkcenterShifts.FindAsync(wsd => wsd.WorkcenterId == workcenterId && wsd.EndTime == null);
            return [.. workcenterShifts];
        }

        public async Task<GenericResponse> CreateWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos)
        {
            // Obtenir els torns actius dels centres de treball indicats en els DTOs
            var currentWorkcenterShifts = await _unitOfWork.WorkcenterShifts
                .FindWithDetails(ws => dtos.Select(dto => dto.WorkcenterId).Contains(ws.WorkcenterId) && ws.EndTime == null)
                .ToListAsync();

            var newWorkcenterShifts = CreateUnexistingWorkcenterShifts(dtos, currentWorkcenterShifts);
            var newWorkcenterShiftDetails = new List<WorkcenterShiftDetail>();

            foreach (var workcenterShift in currentWorkcenterShifts)
            {
                var dto = dtos.First(dto => dto.WorkcenterId == workcenterShift.WorkcenterId)!;

                workcenterShift.EndTime = dto.StartTime;
                _unitOfWork.WorkcenterShifts.UpdateWithoutSave(workcenterShift);

                var newWorkcenterShift = new WorkcenterShift()
                {
                    WorkcenterId = dto.WorkcenterId,
                    ShiftDetailId = dto.ShiftDetailId,
                    StartTime = dto.StartTime,
                    EndTime = null
                };
                newWorkcenterShifts.Add(newWorkcenterShift);

                foreach (var detail in workcenterShift.Details)
                {
                    detail.EndTime = dto.StartTime;
                    _unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(detail);

                    newWorkcenterShiftDetails.Add(new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = newWorkcenterShift.Id,
                        MachineStatusId = detail.MachineStatusId,
                        OperatorId = detail.OperatorId,
                        WorkOrderPhaseId = detail.WorkOrderPhaseId,
                        StartTime = dto.StartTime,
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

        public async Task<GenericResponse> CreateWorkcenterShiftDetail(CreateWorkcenterShiftDetailDto dto)
        {
            var moment = DateTime.Now;

            var currentWorkcenterShift = (await _unitOfWork.WorkcenterShifts.FindAsync(ws => ws.WorkcenterId == dto.WorkcenterId && ws.EndTime == null)).FirstOrDefault();
            if (currentWorkcenterShift == null)
            {
                return new GenericResponse(false, "No open workcenter shift found");
            }

            var currentDetail = (await _unitOfWork.WorkcenterShifts.Details.FindAsync(wsd => wsd.WorkcenterShiftId == currentWorkcenterShift.Id && wsd.EndTime == null)).FirstOrDefault();
            if (currentDetail != null)
            {
                currentDetail.EndTime = moment;
                _unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentDetail);
            }

            var workcenterShiftDetail = new WorkcenterShiftDetail
            {
                WorkcenterShiftId = currentWorkcenterShift.Id,
                MachineStatusId = dto.MachineStatusId,
                OperatorId = dto.OperatorId,
                WorkOrderPhaseId = dto.WorkOrderPhaseId,
                StartTime = moment
            };
            await _unitOfWork.WorkcenterShifts.Details.Add(workcenterShiftDetail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto)
        {
            // Obtener el último detalle de la fase de la orden de trabajo
            var recentDetail = await _unitOfWork.WorkcenterShifts
                                                .FindWithDetails(ws => ws.WorkcenterId == dto.WorkcenterId)
                                                .SelectMany(ws => ws.Details
                                                    .Where(wsd => wsd.WorkOrderPhaseId == dto.WorkOrderPhaseId))
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

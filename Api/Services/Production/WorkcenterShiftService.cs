﻿using Application.Contracts;
using Application.Contracts.Production;
using Application.Persistance;
using Application.Services.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Api.Services.Production
{
    public class WorkcenterShiftService(IUnitOfWork unitOfWork) : IWorkcenterShiftService
    {
        public IWorkcenterShiftDetailService DetailsService => new WorkcenterShiftDetailService(unitOfWork);

        public async Task<WorkcenterShift?> GetWorkcenterShift(Guid workcenterShiftId)
        {
            var workcenterShift = await unitOfWork.WorkcenterShifts.Get(workcenterShiftId);
            return workcenterShift;
        }

        public async Task<List<WorkcenterShift>> GetCurrentWorkcenterShifts()
        {
            var workcenterShifts = await unitOfWork.WorkcenterShifts.FindWithDetails(wsd => wsd.Current).ToListAsync();
            return [.. workcenterShifts];
        }

        public async Task<GenericResponse> CreateWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos)
        {
            // Obtenir els torns actius dels centres de treball indicats en els DTOs
            var currentWorkcenterShifts = await unitOfWork.WorkcenterShifts
                .FindWithDetails(ws => dtos.Select(dto => dto.WorkcenterId).Contains(ws.WorkcenterId) && ws.Current)
                .ToListAsync();

            var newWorkcenterShifts = CreateUnexistingWorkcenterShifts(dtos, currentWorkcenterShifts);
            var newWorkcenterShiftDetails = new List<WorkcenterShiftDetail>();

            foreach (var workcenterShift in currentWorkcenterShifts)
            {
                var dto = dtos.First(dto => dto.WorkcenterId == workcenterShift.WorkcenterId)!;

                workcenterShift.Current = false;
                workcenterShift.EndTime = dto.StartTime;
                unitOfWork.WorkcenterShifts.UpdateWithoutSave(workcenterShift);

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
                    unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(detail);

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

            await unitOfWork.WorkcenterShifts.AddRangeWithoutSave(newWorkcenterShifts);
            await unitOfWork.WorkcenterShifts.Details.AddRangeWithoutSave(newWorkcenterShiftDetails);

            await unitOfWork.CompleteAsync();

            return new GenericResponse(true);
        }

        private static List<WorkcenterShift> CreateUnexistingWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos, List<WorkcenterShift> currentWorkcenterShifts)
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

        public async Task<GenericResponse> DisableWorkcenterShift(Guid workcenterShiftId)
        {
            var workcenterShift = await unitOfWork.WorkcenterShifts.Get(workcenterShiftId);
            if (workcenterShift == null)
            {
                return new GenericResponse(false, "Workcenter shift not found");
            }
            workcenterShift.Disabled = true;
            unitOfWork.WorkcenterShifts.UpdateWithoutSave(workcenterShift);

            var workcenterShiftDetails = await unitOfWork.WorkcenterShifts.Details.FindAsync(wsd => wsd.WorkcenterShiftId == workcenterShiftId);
            foreach (var workcenterShiftDetail in workcenterShiftDetails)
            {
                workcenterShiftDetail.Disabled = true;
                unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(workcenterShiftDetail);
            }

            await unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }


    }
}

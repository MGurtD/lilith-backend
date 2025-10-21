using Application.Contracts;
using Application.Contracts.Production;
using Application.Persistance;
using Application.Services.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Api.Services.Production
{
    public class WorkcenterShiftService(IUnitOfWork unitOfWork, IMetricsService metricsService) : IWorkcenterShiftService
    {
        public IWorkcenterShiftDetailService DetailsService => new WorkcenterShiftDetailService(unitOfWork, metricsService);

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
            var currentWorkcenterShifts = (await GetCurrentWorkcenterShifts())
                .Where(ws => dtos.Select(dto => dto.WorkcenterId).Contains(ws.WorkcenterId))
                .ToList();

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

                foreach (var detail in workcenterShift.Details.Where(wsd => wsd.Current))
                {
                    detail.Current = false;
                    detail.EndTime = dto.StartTime;
                    unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(detail);

                    newWorkcenterShiftDetails.Add(new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = newWorkcenterShift.Id,
                        MachineStatusId = detail.MachineStatusId,
                        WorkcenterCost = detail.WorkcenterCost,
                        OperatorId = detail.OperatorId,
                        OperatorCost = detail.OperatorCost,
                        ConcurrentOperatorWorkcenters = detail.ConcurrentOperatorWorkcenters,
                        WorkOrderPhaseId = detail.WorkOrderPhaseId,
                        ConcurrentWorkorderPhases = detail.ConcurrentWorkorderPhases,
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
        

        public async Task<List<WorkcenterShiftDetailResponseDto>> GetWorkcenterShiftDetails(WorkcenterShiftDetailsQueryDto query)
        {
            // Obtenir tots els detalls dins del rang de dates
            var allDetails = await unitOfWork.WorkcenterShifts.Details.FindAsync(d =>
                !d.Disabled &&
                !d.WorkcenterShift!.Disabled &&
                (
                    (d.StartTime >= query.StartDate && d.StartTime <= query.EndDate) ||
                    (d.EndTime.HasValue && d.EndTime.Value >= query.StartDate && d.EndTime.Value <= query.EndDate) ||
                    (d.StartTime <= query.StartDate && (!d.EndTime.HasValue || d.EndTime.Value >= query.EndDate))
                )
            );

            // Aplicar filtres opcionals
            var filteredDetails = allDetails.AsEnumerable();

            if (query.WorkcenterId.HasValue)
            {
                filteredDetails = filteredDetails.Where(d => d.WorkcenterShift!.WorkcenterId == query.WorkcenterId.Value);
            }

            if (query.OperatorId.HasValue)
            {
                filteredDetails = filteredDetails.Where(d => d.OperatorId == query.OperatorId.Value);
            }

            if (query.WorkOrderPhaseId.HasValue)
            {
                filteredDetails = filteredDetails.Where(d => d.WorkOrderPhaseId == query.WorkOrderPhaseId.Value);
            }

            var detailsList = filteredDetails.ToList();

            // Carregar les entitats relacionades
            var workcenterShiftIds = detailsList.Select(d => d.WorkcenterShiftId).Distinct().ToList();
            var workcenterShifts = new Dictionary<Guid, WorkcenterShift>();
            foreach (var id in workcenterShiftIds)
            {
                var ws = await unitOfWork.WorkcenterShifts.Get(id);
                if (ws != null) workcenterShifts[id] = ws;
            }

            var workcenterIds = workcenterShifts.Values.Select(ws => ws.WorkcenterId).Distinct().ToList();
            var workcenters = new Dictionary<Guid, Workcenter>();
            foreach (var id in workcenterIds)
            {
                var wc = await unitOfWork.Workcenters.Get(id);
                if (wc != null) workcenters[id] = wc;
            }

            var machineStatusIds = detailsList.Select(d => d.MachineStatusId).Distinct().ToList();
            var machineStatuses = new Dictionary<Guid, MachineStatus>();
            foreach (var id in machineStatusIds)
            {
                var ms = await unitOfWork.MachineStatuses.Get(id);
                if (ms != null) machineStatuses[id] = ms;
            }

            var operatorIds = detailsList.Where(d => d.OperatorId.HasValue).Select(d => d.OperatorId!.Value).Distinct().ToList();
            var operators = new Dictionary<Guid, Operator>();
            foreach (var id in operatorIds)
            {
                var op = await unitOfWork.Operators.Get(id);
                if (op != null) operators[id] = op;
            }

            var workOrderPhaseIds = detailsList.Where(d => d.WorkOrderPhaseId.HasValue).Select(d => d.WorkOrderPhaseId!.Value).Distinct().ToList();
            var workOrderPhases = new Dictionary<Guid, WorkOrderPhase>();
            foreach (var id in workOrderPhaseIds)
            {
                var wop = await unitOfWork.WorkOrders.Phases.Get(id);                    
                if (wop != null) workOrderPhases[id] = wop;
            }

            // Mapear a DTOs amb les relacions carregades
            return detailsList.Select(d => MapToResponseDto(
                d,
                query.StartDate,
                query.EndDate,
                workcenterShifts.GetValueOrDefault(d.WorkcenterShiftId),
                workcenters,
                machineStatuses.GetValueOrDefault(d.MachineStatusId),
                operators.GetValueOrDefault(d.OperatorId ?? Guid.Empty),
                workOrderPhases.GetValueOrDefault(d.WorkOrderPhaseId ?? Guid.Empty)
            )).ToList();
        }

        public async Task<List<GroupedWorkcenterShiftDetailsDto>> GetGroupedWorkcenterShiftDetails(WorkcenterShiftDetailsQueryDto query)
        {
            var details = await GetWorkcenterShiftDetails(query);

            if (query.GroupBy == null || query.GroupBy == GroupBy.None)
            {
                return
                [
                    new GroupedWorkcenterShiftDetailsDto
            {
                GroupKey = "All",
                Details = details,
                TotalHours = details.Sum(d => d.TotalHours),
                TotalCost = details.Sum(d => d.TotalCost),
                TotalQuantityOk = details.Sum(d => d.QuantityOk),
                TotalQuantityKo = details.Sum(d => d.QuantityKo),
                DetailCount = details.Count
            }
                ];
            }

            IEnumerable<IGrouping<(string Key, Guid? Id), WorkcenterShiftDetailResponseDto>> grouped = query.GroupBy switch
            {
                GroupBy.Workcenter => details.GroupBy(d => (d.WorkcenterName ?? "Unknown", (Guid?)d.WorkcenterId)),
                GroupBy.Operator => details.GroupBy(d => (d.OperatorName ?? "No Operator", d.OperatorId)),
                GroupBy.WorkOrderPhase => details.GroupBy(d => (d.WorkOrderCode != null ? $"{d.WorkOrderCode} - {d.WorkOrderPhaseName}" : "No Work Order", d.WorkOrderPhaseId)),
                _ => details.GroupBy(d => ("All", (Guid?)null))
            };

            return grouped.Select(g => new GroupedWorkcenterShiftDetailsDto
            {
                GroupKey = g.Key.Key,
                GroupId = g.Key.Id,
                Details = g.ToList(),
                TotalHours = g.Sum(d => d.TotalHours),
                TotalCost = g.Sum(d => d.TotalCost),
                TotalQuantityOk = g.Sum(d => d.QuantityOk),
                TotalQuantityKo = g.Sum(d => d.QuantityKo),
                DetailCount = g.Count()
            }).OrderByDescending(g => g.TotalHours).ToList();
        }

        private static WorkcenterShiftDetailResponseDto MapToResponseDto(
            WorkcenterShiftDetail detail,
            DateTime queryStartDate,
            DateTime queryEndDate,
            WorkcenterShift? workcenterShift,
            Dictionary<Guid, Workcenter> workcenters,
            MachineStatus? machineStatus,
            Operator? operatorEntity,
            WorkOrderPhase? workOrderPhase)
        {
            // Calcula les hores dins del rang de dates
            var effectiveStartTime = detail.StartTime < queryStartDate ? queryStartDate : detail.StartTime;
            var effectiveEndTime = detail.EndTime.HasValue
                ? detail.EndTime.Value
                : effectiveStartTime;

            var totalHours = (decimal)(effectiveEndTime - effectiveStartTime).TotalHours;

            // Calcula el cost total considerant els workcenters i fases concurrents
            var operatorCostShare = detail.OperatorId.HasValue && detail.ConcurrentOperatorWorkcenters > 0
                ? detail.OperatorCost / detail.ConcurrentOperatorWorkcenters
                : 0;

            var workcenterCostShare = detail.ConcurrentWorkorderPhases > 0
                ? detail.WorkcenterCost / detail.ConcurrentWorkorderPhases
                : detail.WorkcenterCost;

            var totalCost = (operatorCostShare + workcenterCostShare) * totalHours;

            var workcenter = workcenterShift != null ? workcenters.GetValueOrDefault(workcenterShift.WorkcenterId) : null;

            return new WorkcenterShiftDetailResponseDto
            {
                Id = detail.Id,
                WorkcenterShiftId = detail.WorkcenterShiftId,
                StartTime = detail.StartTime,
                EndTime = detail.EndTime,
                Current = detail.Current,

                MachineStatusId = detail.MachineStatusId,
                MachineStatusName = machineStatus?.Name,

                WorkcenterId = workcenterShift?.WorkcenterId ?? Guid.Empty,
                WorkcenterName = workcenter?.Name,
                WorkcenterCost = detail.WorkcenterCost,

                OperatorId = detail.OperatorId,
                OperatorName = operatorEntity?.Surname + ", " + operatorEntity?.Name,
                OperatorCost = detail.OperatorCost,
                ConcurrentOperatorWorkcenters = detail.ConcurrentOperatorWorkcenters,

                WorkOrderPhaseId = detail.WorkOrderPhaseId,
                WorkOrderPhaseName = workOrderPhase?.Description,
                WorkOrderCode = workOrderPhase?.WorkOrder?.Code,
                ConcurrentWorkorderPhases = detail.ConcurrentWorkorderPhases,

                QuantityOk = detail.QuantityOk,
                QuantityKo = detail.QuantityKo,

                TotalHours = Math.Max(0, totalHours),
                TotalCost = Math.Max(0, totalCost)
            };
        }

    }
}

using Application.Contracts.Production;
using Application.Persistance;
using Application.Services.Production;

namespace Api.Services.Production
{
    public class WorkOrderReportService(IUnitOfWork unitOfWork) : IWorkOrderReportService
    {
        public async Task<WorkOrderReportResponse?> GetReportById(Guid id)
        {
            var workOrder = await unitOfWork.WorkOrders.GetDetailed(id);
            if (workOrder == null) return null;

            var status = await unitOfWork.Lifecycles.StatusRepository.Get(workOrder.StatusId);

            var orderDto = new WorkOrderReportDto()
            {
                Code = workOrder.Code,
                ReferenceCode = workOrder.Reference!.Code,
                ReferenceDescription = workOrder.Reference!.Description,
                PlannedDate = workOrder.PlannedDate,
                PlannedQuantity = workOrder.PlannedQuantity,
                TotalQuantity = workOrder.TotalQuantity,
                StatusName = status?.Name ?? string.Empty,
                OperatorCost = workOrder.OperatorCost,
                MachineCost = workOrder.MachineCost,
                MaterialCost = workOrder.MaterialCost,
                TotalCost = workOrder.OperatorCost + workOrder.MachineCost + workOrder.MaterialCost
            };

            var phaseDtos = new List<WorkOrderPhaseReportDto>();
            foreach (var phase in workOrder.Phases.Where(p => !p.Disabled).OrderBy(p => p.Code))
            {
                var phaseStatus = await unitOfWork.Lifecycles.StatusRepository.Get(phase.StatusId);
                var operatorType = phase.OperatorTypeId.HasValue 
                    ? await unitOfWork.OperatorTypes.Get(phase.OperatorTypeId.Value) 
                    : null;
                var workcenterType = phase.WorkcenterTypeId.HasValue 
                    ? await unitOfWork.WorkcenterTypes.Get(phase.WorkcenterTypeId.Value) 
                    : null;

                var detailDtos = new List<WorkOrderPhaseDetailReportDto>();
                foreach (var detail in phase.Details.Where(d => !d.Disabled).OrderBy(d => d.Order))
                {
                    detailDtos.Add(new WorkOrderPhaseDetailReportDto()
                    {
                        Description = detail.Comment,
                        WorkcenterTypeName = workcenterType?.Name ?? string.Empty,
                        OperatorTypeName = operatorType?.Name ?? string.Empty,
                        EstimatedTime = detail.EstimatedTime,
                        RealTime = 0 // Real time would come from production parts if tracked
                    });
                }

                var bomDtos = new List<WorkOrderPhaseBillOfMaterialsReportDto>();
                foreach (var bom in phase.BillOfMaterials.Where(b => !b.Disabled))
                {
                    var bomReference = await unitOfWork.References.Get(bom.ReferenceId);
                    if (bomReference != null)
                    {
                        bomDtos.Add(new WorkOrderPhaseBillOfMaterialsReportDto()
                        {
                            ReferenceCode = bomReference.Code,
                            ReferenceDescription = bomReference.Description,
                            Quantity = bom.Quantity,
                            Width = bom.Width,
                            Length = bom.Length,
                            Thickness = bom.Thickness,
                            Diameter = bom.Diameter
                        });
                    }
                }

                phaseDtos.Add(new WorkOrderPhaseReportDto()
                {
                    Code = phase.Code,
                    Description = phase.Description,
                    StatusName = phaseStatus?.Name ?? string.Empty,
                    OperatorTime = phase.Details.Sum(d => d.EstimatedOperatorTime),
                    MachineTime = phase.Details.Sum(d => d.EstimatedTime),
                    ExternalWorkCost = phase.ExternalWorkCost,
                    TransportCost = phase.TransportCost,
                    Details = detailDtos,
                    BillOfMaterials = bomDtos
                });
            }

            return new WorkOrderReportResponse()
            {
                Order = orderDto,
                Phases = phaseDtos
            };
        }
    }
}

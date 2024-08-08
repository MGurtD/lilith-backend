using Api.Services.Sales;
using Application.Contracts;
using Application.Contracts.Production;
using Application.Persistance;
using Application.Production.Warehouse;
using Application.Services;
using Application.Services.Sales;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using System.Collections;

namespace Api.Services.Production
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExerciseService _exerciseService;
        private readonly ISalesOrderService _salesOrderService;

        public WorkOrderService(IUnitOfWork unitOfWork, IExerciseService exerciseService, ISalesOrderService salesOrderService)
        {
            _unitOfWork = unitOfWork;
            _exerciseService = exerciseService;
            _salesOrderService = salesOrderService;
        }        

        public IEnumerable<DetailedWorkOrder> GetWorkOrderDetails(Guid id) {
            var details = _unitOfWork.DetailedWorkOrders.Find(d => d.WorkOrderId == id);

            var groupedDetails =
            from d in details
            group d by new
            {
                d.WorkOrderId,
                d.WorkOrderCode,
                d.WorkOrderStartTime,
                d.WorkOrderEndTime,
                d.WorkOrderStatusCode,
                d.WorkOrderStatusDescription,
                d.WorkOrderOrder,
                d.WorkOrderComment,
                d.PlannedDate,
                d.PlannedQuantity,
                d.ReferenceCode,
                d.ReferenceDescription,
                d.ReferenceVersion,
                d.ReferenceCost,
                d.WorkOrderPhaseId,
                d.WorkOrderPhaseCode,
                d.WorkOrderPhaseDescription,
                d.WorkOrderPhaseComment,
                d.WorkOrderPhaseStartTime,
                d.WorkOrderPhaseEndTime,
                d.WorkOrderPhaseStatusCode,
                d.WorkOrderPhaseStatusDescription,
                d.WorkOrderPhaseDetailId,
                d.WorkOrderPhaseDetailOrder,
                d.WorkOrderPhaseDetailEstimatedTime,
                d.WorkOrderPhaseDetailComment,
                d.MachineStatusName,
                d.MachineStatusDescription,
            } into gcs
            select new DetailedWorkOrder()
            {
                WorkOrderId = gcs.Key.WorkOrderId,
                WorkOrderCode = gcs.Key.WorkOrderCode,
                WorkOrderStartTime = gcs.Key.WorkOrderStartTime,
                WorkOrderEndTime = gcs.Key.WorkOrderEndTime,
                WorkOrderStatusCode = gcs.Key.WorkOrderStatusCode,
                WorkOrderStatusDescription = gcs.Key.WorkOrderStatusDescription,
                WorkOrderOrder = gcs.Key.WorkOrderOrder,
                WorkOrderComment = gcs.Key.WorkOrderComment,
                PlannedDate = gcs.Key.PlannedDate,
                PlannedQuantity = gcs.Key.PlannedQuantity,
                ReferenceCode = gcs.Key.ReferenceCode,
                ReferenceDescription = gcs.Key.ReferenceDescription,
                ReferenceVersion = gcs.Key.ReferenceVersion,
                ReferenceCost = gcs.Key.ReferenceCost,
                WorkOrderPhaseId = gcs.Key.WorkOrderPhaseId,
                WorkOrderPhaseCode = gcs.Key.WorkOrderPhaseCode,
                WorkOrderPhaseDescription = gcs.Key.WorkOrderPhaseDescription,
                WorkOrderPhaseComment = gcs.Key.WorkOrderPhaseComment,
                WorkOrderPhaseStartTime = gcs.Key.WorkOrderPhaseStartTime,
                WorkOrderPhaseEndTime = gcs.Key.WorkOrderPhaseEndTime,
                WorkOrderPhaseStatusCode = gcs.Key.WorkOrderPhaseStatusCode,
                WorkOrderPhaseStatusDescription = gcs.Key.WorkOrderPhaseStatusDescription,
                WorkOrderPhaseDetailId = gcs.Key.WorkOrderPhaseDetailId,
                WorkOrderPhaseDetailOrder = gcs.Key.WorkOrderPhaseDetailOrder,
                WorkOrderPhaseDetailEstimatedTime = gcs.Key.WorkOrderPhaseDetailEstimatedTime,
                WorkOrderPhaseDetailComment = gcs.Key.WorkOrderPhaseDetailComment,
                MachineStatusName = gcs.Key.MachineStatusName,
                MachineStatusDescription = gcs.Key.MachineStatusDescription,
                PreferredWorkcenter = false,
                WorkcenterId = Guid.Empty,
                WorkcenterName = "",
                WorkcenterCost = 0,
                WorkcenterDescription = ""
            };

            return groupedDetails;
        }

        public async Task<GenericResponse> CreateFromWorkMaster(CreateWorkOrderDto dto)
        {
            // Ruta de fabricació
            var workMaster = await _unitOfWork.WorkMasters.GetFullById(dto.WorkMasterId);
            if (workMaster is null) return new GenericResponse(false, $"La ruta de fabricació no existeix");

            // Exercici
            var exercise = _exerciseService.GetExerciceByDate(dto.PlannedDate);
            if (exercise is null) return new GenericResponse(false, $"No hi ha exercici creat per la data planificada");
            var exerciseServiceResponse = await _exerciseService.GetNextCounter(exercise.Id, "workorder");
            if (!exerciseServiceResponse.Result || exerciseServiceResponse.Content is null) return new GenericResponse(false, $"No s'ha pugut generar correctament el codi de la ordre");
            var code = Convert.ToString(exerciseServiceResponse.Content);

            // Estat
            var initialStatusId = await GetInitialStatus();
            if (initialStatusId is null) return new GenericResponse(false, $"No hi ha un estat inicial definit al cicle de vida WorkOrder");

            // Crear ordre de fabricació
            var workOrder = new WorkOrder()
            { 
                WorkMasterId = dto.WorkMasterId,
                Code = code!,
                ExerciseId = exercise.Id,
                ReferenceId = workMaster.ReferenceId,
                PlannedDate = dto.PlannedDate,
                PlannedQuantity = dto.PlannedQuantity,
                StatusId = initialStatusId.Value,
                Comment = dto.Comment
            };

            foreach (var workMasterPhase in workMaster.Phases) 
            {
                // Afegir fases
                var workOrderPhase = new WorkOrderPhase()
                {
                    Code = workMasterPhase.Code,
                    Description = workMasterPhase.Description,
                    Comment = workMasterPhase.Comment,
                    ExternalWorkCost = workMasterPhase.ExternalWorkCost,
                    IsExternalWork = workMasterPhase.IsExternalWork,
                    TransportCost = workMasterPhase.TransportCost,
                    ServiceReferenceId = workMasterPhase.ServiceReferenceId,
                    OperatorTypeId = workMasterPhase.OperatorTypeId,
                    PreferredWorkcenterId = workMasterPhase.PreferredWorkcenterId,
                    WorkcenterTypeId = workMasterPhase.WorkcenterTypeId,
                    ProfitPercentage = workMasterPhase.ProfitPercentage,
                    WorkOrderId = workOrder.Id,
                    StatusId = initialStatusId.Value
                };

                // Afegir detalls de la fase
                foreach (var workMasterPhaseDetail in workMasterPhase.Details)
                {
                    var workOrderPhaseDetail = new WorkOrderPhaseDetail()
                    {
                        WorkOrderPhaseId = workOrderPhase.Id,
                        EstimatedTime = workMasterPhaseDetail.EstimatedTime,
                        EstimatedOperatorTime = workMasterPhaseDetail.EstimatedOperatorTime,
                        Comment = workMasterPhaseDetail.Comment,
                        IsCycleTime = workMasterPhaseDetail.IsCycleTime,
                        MachineStatusId = workMasterPhaseDetail.MachineStatusId,
                        Order = workMasterPhaseDetail.Order                        
                    };
                    workOrderPhase.Details.Add(workOrderPhaseDetail);
                }

                // Afegir llista de materials de la fase
                foreach (var workMasterPhaseBomItem in workMasterPhase.BillOfMaterials)
                {
                    var workOrderPhaseBomItem = new WorkOrderPhaseBillOfMaterials()
                    {
                        WorkOrderPhaseId = workOrderPhase.Id,
                        Quantity = dto.PlannedQuantity * workMasterPhaseBomItem.Quantity,
                        Diameter = workMasterPhaseBomItem.Diameter,
                        Height = workMasterPhaseBomItem.Height,
                        Length = workMasterPhaseBomItem.Length,
                        ReferenceId = workMasterPhaseBomItem.ReferenceId,
                        Thickness = workMasterPhaseBomItem.Thickness,
                        Width = workMasterPhaseBomItem.Width,
                    };
                    workOrderPhase.BillOfMaterials.Add(workOrderPhaseBomItem);
                }

                workOrder.Phases.Add(workOrderPhase);
            }

            // Guardar ordre de fabricació a la BDD
            await _unitOfWork.WorkOrders.Add(workOrder);

            return new GenericResponse(true, workOrder);
        }

        private async Task<Guid?> GetInitialStatus()
        {
            var status = await _unitOfWork.Lifecycles.GetStatusByName("WorkOrder", "Creada");
            if (status != null) return status.Id;
            return null;
        }

        public async Task<GenericResponse> Start(Guid id)
        {
            var workOrder = await _unitOfWork.WorkOrders.Get(id);
            if (workOrder is null) return new GenericResponse(false, $"La ordre de fabricació no existeix");

            if (!workOrder.StartTime.HasValue) workOrder.StartTime = DateTime.Now;
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> End(Guid id)
        {
            var workOrder = await _unitOfWork.WorkOrders.Get(id);
            if (workOrder is null) return new GenericResponse(false, $"La ordre de fabricació no existeix");

            if (!workOrder.EndTime.HasValue) workOrder.EndTime = DateTime.Now;
            return new GenericResponse(true);
        }

        public IEnumerable<WorkOrder> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid? statusId)
        {
            var workOrders = _unitOfWork.WorkOrders.Find(w => w.PlannedDate >= startDate && w.PlannedDate <= endDate);
            if (statusId.HasValue) workOrders = workOrders.Where(w => w.StatusId == statusId);

            return workOrders;
        }

        public async Task<IEnumerable<WorkOrder>> GetBySalesOrderId(Guid salesOrderId)
        {
            var workOrders = new List<WorkOrder>();

            var salesOrder = await _salesOrderService.GetById(salesOrderId);
            if (salesOrder != null)
            {
                var workOrderIds = salesOrder.SalesOrderDetails.Where(d => d.WorkOrderId != null).Select(d => d.WorkOrderId);
                workOrders = _unitOfWork.WorkOrders.Find(w => workOrderIds.Contains(w.Id)).ToList();
            }

            return workOrders;
        }

        public async Task<GenericResponse> Delete(Guid id)
        {
            var woOrderDetails = _unitOfWork.SalesOrderDetails.Find(d => d.WorkOrderId == id);
                    if (woOrderDetails.Any()) {
                var orderDetail = woOrderDetails.FirstOrDefault();

                if (orderDetail is not null) {
                    orderDetail.WorkOrderId = null;
                    await _unitOfWork.SalesOrderDetails.Update(orderDetail);
                }
            }

            var entity = _unitOfWork.WorkOrders.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return new GenericResponse(false, "La ordre de fabricació no existeix");

            await _unitOfWork.WorkOrders.Remove(entity);
            return new GenericResponse(true, entity);
        }
        
        private enum OperationType
        {
            Add,
            Remove
        }

        private async Task<GenericResponse> UpdateWorkOrderTotalsFromProductionPart(Guid id, ProductionPart productionPart, OperationType operationType)
        {
            var workOrder = await _unitOfWork.WorkOrders.Get(id);            
            if (workOrder is null) return new GenericResponse(false, $"La ordre de fabricació no existeix");

            if (operationType == OperationType.Add) {
                workOrder.OperatorTime += productionPart.OperatorTime;
                workOrder.MachineTime += productionPart.WorkcenterTime;
                workOrder.TotalQuantity += productionPart.Quantity;
                workOrder.MachineCost += (productionPart.WorkcenterTime / 60) * productionPart.MachineHourCost;
                workOrder.OperatorCost += (productionPart.OperatorTime/60) * productionPart.OperatorHourCost;
            } else {
                workOrder.OperatorTime -= productionPart.OperatorTime;
                workOrder.MachineTime -= productionPart.WorkcenterTime;
                workOrder.TotalQuantity -= productionPart.Quantity;
                workOrder.MachineCost -= (productionPart.WorkcenterTime / 60) * productionPart.MachineHourCost;
                workOrder.OperatorCost -= (productionPart.OperatorTime/60) * productionPart.OperatorHourCost;
            }

            await _unitOfWork.WorkOrders.Update(workOrder);
            return new GenericResponse(true , workOrder);
        }

        public async Task<GenericResponse> AddProductionPart(Guid id, ProductionPart productionPart)
        {
            return await UpdateWorkOrderTotalsFromProductionPart(id, productionPart, OperationType.Add);
        }

        public async Task<GenericResponse> RemoveProductionPart(Guid id, ProductionPart productionPart)
        {
            return await UpdateWorkOrderTotalsFromProductionPart(id, productionPart, OperationType.Remove);
        }

        public async Task Update(WorkOrder workOrder)
        {
            var lastCost = workOrder.MachineCost + workOrder.OperatorCost + workOrder.MaterialCost;
            var reference = await _unitOfWork.References.Get(workOrder.ReferenceId);
            if (reference != null)
            {
                reference.LastCost = lastCost;
                await _unitOfWork.References.Update(reference);
            }

            var details = _unitOfWork.SalesOrderDetails.Find(e => e.WorkOrderId == workOrder.Id).ToList();
            foreach (SalesOrderDetail detail in details)
            {
                detail.LastCost = lastCost;
                await _unitOfWork.SalesOrderDetails.Update(detail);
            }
            
            await _unitOfWork.WorkOrders.Update(workOrder);
        }
    }
}

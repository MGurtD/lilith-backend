using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkOrderService
{
    Task<WorkOrder?> GetById(Guid id);
    Task<GenericResponse> Create(WorkOrder workOrder);
    Task<GenericResponse> CreateFromWorkMaster(CreateWorkOrderDto dto);
    Task<GenericResponse> Start(Guid id);
    Task<GenericResponse> End(Guid id);
    Task<GenericResponse> Delete(Guid id); 
    IEnumerable<WorkOrder> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid? statusId);
    IEnumerable<DetailedWorkOrder> GetWorkOrderDetails(Guid id);
    Task<IEnumerable<WorkOrder>> GetBySalesOrderId(Guid salesOrderId);
    Task<GenericResponse> AddProductionPart(Guid id, ProductionPart productionPart);
    Task<GenericResponse> RemoveProductionPart(Guid id, ProductionPart productionPart);
    Task Update(WorkOrder workOrder);
    Task<IEnumerable<WorkOrder>> GetPlannableWorkOrders();
    Task<GenericResponse> Priorize(List<UpdateWorkOrderOrderDTO> orders);
}

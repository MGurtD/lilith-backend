using Application.Contracts;
using Application.Contracts.Production;
using Domain.Entities.Production;

namespace Application.Production.Warehouse
{
    public interface IWorkOrderService
    {
        Task<GenericResponse> CreateFromWorkMaster(CreateWorkOrderDto dto);
        Task<GenericResponse> Start(Guid id);
        Task<GenericResponse> End(Guid id); 
        IEnumerable<WorkOrder> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid? statusId);
        Task<IEnumerable<WorkOrder>> GetBySalesOrderId(Guid salesOrderId);

    }
}
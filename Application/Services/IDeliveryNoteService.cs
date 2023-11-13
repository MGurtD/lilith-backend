using Application.Contracts;
using Application.Contracts.Sales;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;

namespace Application.Services
{
    public interface IDeliveryNoteService
    {
        IEnumerable<DeliveryNote> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<DeliveryNote> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);
        IEnumerable<DeliveryNote> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId);

        Task<GenericResponse> Create(CreateHeaderRequest createRequest);
        Task<GenericResponse> Update(DeliveryNote deliveryNote);
        Task<GenericResponse> Remove(Guid id);
        
        Task<GenericResponse> MoveToWarehose(DeliveryNote deliveryNote);
        Task<GenericResponse> RetriveFromWarehose(DeliveryNote deliveryNote);

        Task<GenericResponse> AddOrder(Guid deliveryNoteId, SalesOrderHeader order);
        Task<GenericResponse> RemoveOrder(Guid deliveryNoteId, SalesOrderHeader order);
    }
}
using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Services.Warehouse
{
    public interface IStockMovementService
    {
        Task<GenericResponse> Create(StockMovement stockMovement);
        IEnumerable<StockMovement> GetBetweenDates(DateTime startDate, DateTime endDate, Guid? locationId);
        Task<GenericResponse> Remove(Guid id);
    }
}
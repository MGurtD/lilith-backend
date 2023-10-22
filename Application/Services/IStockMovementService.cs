using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Services
{
    public interface IStockMovementService
    {
        Task<GenericResponse> Create(StockMovement stockMovement);
        IEnumerable<StockMovement> GetBetweenDates(DateTime startDate, DateTime endDate);
    }
}
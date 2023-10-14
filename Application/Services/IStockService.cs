using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Services
{
    public interface IStockService
    {
        Task<GenericResponse> Create(Stock request);
        Task<GenericResponse> Update(Stock request);
        IEnumerable<Stock> GetByLocation(Guid locationId);
        IEnumerable<Stock> GetByReference(Guid referenceId);
        Task<IEnumerable<Stock>> GetAll();
    }
}
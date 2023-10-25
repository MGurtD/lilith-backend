using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Services
{
    public interface IStockService
    {
        Task<GenericResponse> Create(Stock request);
        Task<GenericResponse> Update(Stock request);
        Task<GenericResponse> Delete(Stock request);
        IEnumerable<Stock> GetByLocation(Guid locationId);
        IEnumerable<Stock> GetByReference(Guid referenceId);
        Task<Stock>GetByDimensions(Guid locationId, Guid referenceId, decimal width, decimal length, decimal height, decimal diameter, decimal thickness);
        Task<IEnumerable<Stock>> GetAll();
        
    }
}
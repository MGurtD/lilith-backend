using Application.Contracts;
using Domain.Entities.Purchase;
using Domain.Entities.Shared;

namespace Application.Contracts;
public interface IReferenceService
{
    GenericResponse CanDelete(Guid referenceId);

    Task<List<Reference>> GetReferenceByCategory(string categoryName);
    Task <GenericResponse> UpdatePriceFromReceipt(Receipt receipt);
    Task<decimal> GetPrice(Guid referenceId, Guid? supplierId);
}

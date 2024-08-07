using Application.Contracts;
using Domain.Entities.Shared;

namespace Application.Services;
public interface IReferenceService
{
    GenericResponse CanDelete(Guid referenceId);

    Task<List<Reference>> GetReferenceByCategory(string categoryName);
}
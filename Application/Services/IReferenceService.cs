using Application.Contracts;

namespace Application.Services;
public interface IReferenceService
{
    GenericResponse CanDelete(Guid referenceId);
}
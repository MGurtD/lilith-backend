using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Application.Services;

public interface ILifecycleService
{
    // Lifecycle operations
    Task<GenericResponse> CreateLifecycle(Lifecycle lifecycle);
    Task<IEnumerable<Lifecycle>> GetAllLifecycles();
    Task<Lifecycle?> GetLifecycleById(Guid id);
    Task<Lifecycle?> GetLifecycleByName(string name);
    Task<GenericResponse> UpdateLifecycle(Lifecycle lifecycle);
    Task<GenericResponse> RemoveLifecycle(Guid id);

    // Status operations
    Task<GenericResponse> CreateStatus(Status status);
    Task<GenericResponse> UpdateStatus(Status status);
    Task<GenericResponse> RemoveStatus(Guid id);

    // StatusTransition operations
    Task<GenericResponse> CreateStatusTransition(StatusTransition transition);
    Task<GenericResponse> UpdateStatusTransition(StatusTransition transition);
    Task<GenericResponse> RemoveStatusTransition(Guid id);
}

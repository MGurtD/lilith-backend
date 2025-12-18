using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Api.Services.Shared;

public class LifecycleService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ILifecycleService
{
    // Lifecycle operations
    public async Task<GenericResponse> CreateLifecycle(Lifecycle lifecycle)
    {
        var exists = unitOfWork.Lifecycles.Find(r => lifecycle.Name == r.Name).Any();
        if (exists)
        {
            var message = localizationService.GetLocalizedString("EntityAlreadyExists");
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.Add(lifecycle);
        return new GenericResponse(true, lifecycle);
    }

    public async Task<IEnumerable<Lifecycle>> GetAllLifecycles()
    {
        var lifecycles = await unitOfWork.Lifecycles.GetAll();
        return lifecycles.OrderBy(e => e.Name);
    }

    public async Task<Lifecycle?> GetLifecycleById(Guid id)
    {
        return await unitOfWork.Lifecycles.Get(id);
    }

    public async Task<Lifecycle?> GetLifecycleByName(string name)
    {
        return await unitOfWork.Lifecycles.GetByName(name);
    }

    public async Task<GenericResponse> UpdateLifecycle(Lifecycle lifecycle)
    {
        var exists = await unitOfWork.Lifecycles.Exists(lifecycle.Id);
        if (!exists)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", lifecycle.Id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.Update(lifecycle);
        return new GenericResponse(true, lifecycle);
    }

    public async Task<GenericResponse> RemoveLifecycle(Guid id)
    {
        var entity = unitOfWork.Lifecycles.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.Remove(entity);
        return new GenericResponse(true, entity);
    }

    // Status operations
    public async Task<GenericResponse> CreateStatus(Status status)
    {
        var entity = await unitOfWork.Lifecycles.StatusRepository.Get(status.Id);
        if (entity is not null)
        {
            var message = localizationService.GetLocalizedString("EntityAlreadyExists");
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.Add(status);
        return new GenericResponse(true, status);
    }

    public async Task<GenericResponse> UpdateStatus(Status status)
    {
        var exists = unitOfWork.Lifecycles.StatusRepository.Find(s => s.Id == status.Id).FirstOrDefault();
        if (exists is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", status.Id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.Update(status);
        return new GenericResponse(true, status);
    }

    public async Task<GenericResponse> RemoveStatus(Guid id)
    {
        var status = unitOfWork.Lifecycles.StatusRepository.Find(s => s.Id == id).FirstOrDefault();
        if (status is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.Remove(status);
        return new GenericResponse(true, status);
    }

    // StatusTransition operations
    public async Task<GenericResponse> CreateStatusTransition(StatusTransition transition)
    {
        if (!transition.StatusId.HasValue || !transition.StatusToId.HasValue)
        {
            var message = localizationService.GetLocalizedString("Validation.Required", "StatusId and StatusToId");
            return new GenericResponse(false, message);
        }

        var status = await unitOfWork.Lifecycles.StatusRepository.Get(transition.StatusId.Value);
        var statusTo = await unitOfWork.Lifecycles.StatusRepository.Get(transition.StatusToId.Value);
        
        if (status is null || statusTo is null)
        {
            var message = localizationService.GetLocalizedString("StatusNotFound", 
                status is null ? transition.StatusId.Value : transition.StatusToId.Value);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Add(transition);
        return new GenericResponse(true, transition);
    }

    public async Task<GenericResponse> UpdateStatusTransition(StatusTransition transition)
    {
        var existing = unitOfWork.Lifecycles.StatusRepository.TransitionRepository
            .Find(t => t.Id == transition.Id).FirstOrDefault();
        
        if (existing is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", transition.Id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Update(transition);
        return new GenericResponse(true, transition);
    }

    public async Task<GenericResponse> RemoveStatusTransition(Guid id)
    {
        var transition = unitOfWork.Lifecycles.StatusRepository.TransitionRepository
            .Find(t => t.Id == id).FirstOrDefault();
        
        if (transition is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Remove(transition);
        return new GenericResponse(true, transition);
    }
}

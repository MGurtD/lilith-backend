using Application.Contracts;
using Domain.Entities;

namespace Application.Services;

public interface IExerciseService
{
    Task<GenericResponse> GetNextCounter(Guid exerciseId, string counterName);
    Exercise? GetExerciceByDate(DateTime dateTime);
}

using Application.Contracts;
using Domain.Entities;

namespace Application.Contracts;

public interface IExerciseService
{
    Task<GenericResponse> GetNextCounter(Guid exerciseId, string counterName);
    Exercise? GetExerciceByDate(DateTime dateTime);
}

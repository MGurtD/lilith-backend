using Application.Persistance.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class ExerciseRepository : Repository<Exercise, Guid>, IExerciseRepository
    {
        public ExerciseRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

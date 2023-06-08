using Application.Persistance.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class ExerciceRepository : Repository<Exercice, Guid>, IExerciceRepository
    {
        public ExerciceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

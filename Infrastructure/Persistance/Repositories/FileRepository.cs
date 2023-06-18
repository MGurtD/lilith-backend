using Application.Persistance.Repositories;

namespace Infrastructure.Persistance.Repositories
{
    public class FileRepository : Repository<Domain.Entities.File, Guid>, IFileRepository
    {
        public FileRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

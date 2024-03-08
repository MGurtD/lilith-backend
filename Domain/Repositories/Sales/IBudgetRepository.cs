using Domain.Entities.Sales;

namespace Application.Persistance.Repositories.Sales
{
    public interface IBudgetRepository : IRepository<Budget, Guid>
    {
        IRepository<BudgetDetail, Guid> Details { get; }
    }
}


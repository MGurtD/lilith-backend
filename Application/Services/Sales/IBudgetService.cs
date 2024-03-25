using Application.Contracts;
using Application.Contracts.Sales;
using Domain.Entities.Sales;

namespace Application.Services.Sales
{
    public interface IBudgetService
    {
        Task<GenericResponse> Create(CreateHeaderRequest createRequest);

        Task<BudgetReportResponse?> GetByIdForReporting(Guid id);
        Task<Budget?> GetById(Guid id);
        IEnumerable<Budget> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<Budget> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId);
        Task<GenericResponse> Update(Budget budget);
        Task<GenericResponse> Remove(Guid id);

        Task<GenericResponse> AddDetail(BudgetDetail detail);
        Task<GenericResponse> UpdateDetail(BudgetDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);

    }
}

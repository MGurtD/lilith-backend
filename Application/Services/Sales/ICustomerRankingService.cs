using Application.Contracts.Sales;

namespace Application.Services.Sales
{
    public interface ICustomerRankingService
    {
        Task<IEnumerable<CustomerSalesRanking>> GetAnnualRanking(int year);
    }
}

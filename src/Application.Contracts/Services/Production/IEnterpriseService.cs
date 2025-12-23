using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IEnterpriseService
    {
        Task<Site?> GetDefaultSite();
    }
}

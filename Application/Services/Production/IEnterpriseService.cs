using Domain.Entities.Production;

namespace Application.Services.Production
{
    public interface IEnterpriseService
    {
        Task<Site?> GetDefaultSite();
    }
}

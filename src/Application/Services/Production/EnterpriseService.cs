using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class EnterpriseService(IUnitOfWork unitOfWork) : IEnterpriseService
    {
        public async Task<Site?> GetDefaultSite()
        {
            var enterprise = (await unitOfWork.Enterprises.GetAll()).FirstOrDefault();
            if (enterprise == null || !enterprise.DefaultSiteId.HasValue)
                return null;

            return await unitOfWork.Sites.Get(enterprise.DefaultSiteId.Value);
        }
    }
}






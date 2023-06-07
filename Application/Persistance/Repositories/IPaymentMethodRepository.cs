using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories
{
    public interface IPaymentMethodRepository : IRepository<PaymentMethod, Guid>
    {
    }
}

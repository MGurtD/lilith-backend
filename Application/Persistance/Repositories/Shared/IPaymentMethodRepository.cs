using Domain.Entities;

namespace Application.Persistance.Repositories
{
    public interface IPaymentMethodRepository : IRepository<PaymentMethod, Guid>
    {
    }
}

using Application.Persistance.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class PaymentMethodRepository : Repository<PaymentMethod, Guid>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

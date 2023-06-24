using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IPurchaseInvoiceRepository : IRepository<PurchaseInvoice, Guid>
    {

        int GetNextNumber();

    }
}

using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IPurchaseInvoiceRepository : IRepository<PurchaseInvoice, Guid>
    {

        IRepository<PurchaseInvoiceImport, Guid> ImportsRepository { get; }

        Task AddImport(PurchaseInvoiceImport import);
        Task UpdateImport(PurchaseInvoiceImport import);
        Task<bool> RemoveImport(Guid id);

        int GetNextNumber();

    }
}

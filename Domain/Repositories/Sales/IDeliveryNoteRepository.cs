using Domain.Entities.Sales;

namespace Application.Persistance.Repositories.Sales
{
    public interface IDeliveryNoteRepository : IRepository<DeliveryNote, Guid>
    {
        IRepository<DeliveryNoteDetail, Guid> Details { get; }

        IEnumerable<DeliveryNote> GetByInvoiceId(Guid invoiceId);
    }
}

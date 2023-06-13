using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public interface IPurchaseInvoiceService
    {
        IEnumerable<PurchaseInvoice> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<PurchaseInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate);

        

    }
}
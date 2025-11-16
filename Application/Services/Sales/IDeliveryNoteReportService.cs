using Domain.Entities.Sales;

namespace Application.Services.Sales;

public interface IDeliveryNoteReportService
{
    Task<DeliveryNoteReportResponse?> GetReportById(Guid id, bool showPrices = true);
}

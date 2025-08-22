using Domain.Entities.Sales;

namespace Application.Services.Sales;

public interface ISalesOrderReportService
{
    Task<SalesOrderReportResponse?> GetReportById(Guid id);
}

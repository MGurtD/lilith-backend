using System;
using System.Threading.Tasks;
using Application.Contracts.Purchase;

namespace Application.Services.Purchase;

public interface IPurchaseOrderReportService
{
    Task<PurchaseOrderReportResponse?> GetReportById(Guid id);
}

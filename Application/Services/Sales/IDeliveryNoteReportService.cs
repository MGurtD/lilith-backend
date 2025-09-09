using System;
using System.Threading.Tasks;
using Domain.Entities.Sales;

namespace Application.Services.Sales;

public interface IDeliveryNoteReportService
{
    Task<DeliveryNoteReportResponse?> GetReportById(Guid id);
}

using System;
using System.Threading.Tasks;
using Application.Contracts.Production;

namespace Application.Services.Production;

public interface IWorkOrderReportService
{
    Task<WorkOrderReportResponse?> GetReportById(Guid id);
}

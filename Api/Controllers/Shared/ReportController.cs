using Application.Contracts.Shared;
using Application.Persistance;
using FastReport;
using FastReport.Data;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            FastReport.Utils.RegisteredObjects.AddConnection(typeof(PostgresDataConnection));
        }


        [Route("Download")]
        [HttpPost]
        public async Task<IActionResult> Download(ReportRequest reportRequest)
        {
            if (!System.IO.File.Exists(reportRequest.File.Path))
                return NotFound();
            
            foreach (var parameter in reportRequest.Parameters)
                await CreateOrUpdateParameter(parameter);

            using var report = new Report();
            report.Load(reportRequest.File.Path);
            report.Prepare();

            using var ms = new MemoryStream();
            var pdfExport = new PDFSimpleExport();
            pdfExport.Export(report, ms);
            ms.Flush();
            return File(ms.ToArray(), "application/pdf", "Report.pdf");
        }


        private async Task CreateOrUpdateParameter(KeyValueParameter parameter)
        {
            var dbParam = _unitOfWork.Parameters.Find(p => p.Key == parameter.Key).FirstOrDefault();
            if (dbParam == null)
            {
                await _unitOfWork.Parameters.Add(new Domain.Entities.Shared.Parameter()
                {
                    Id = Guid.NewGuid(),
                    Key = parameter.Key,
                    Value = parameter.Value,
                    CreatedOn = DateTime.Now
                });
            } else
            {
                dbParam.UpdatedOn = DateTime.Now;
                dbParam.Value = parameter.Value;
                await _unitOfWork.Parameters.Update(dbParam);
            }
        }
    }
}

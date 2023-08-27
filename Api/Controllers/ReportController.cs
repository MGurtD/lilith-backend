using Application.Contracts.Shared;
using Application.Persistance;
using Application.Services;
using FastReport;
using FastReport.Data;
using FastReport.Export.PdfSimple;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _unitOfWork;

        public ReportController(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;

            FastReport.Utils.RegisteredObjects.AddConnection(typeof(PostgresDataConnection));
        }

        [HttpGet]
        public IActionResult Get()
        {
            using (var report = new Report())
            {
                report.Load("C:\\Users\\mgurt\\Desktop\\OneDrive - ENGINYERIA MAPEX S.L\\Customers\\Temges\\Development\\Reports\\SalesInvoice.frx");

                report.SetParameterValue("InvoiceId", "22795052-1b5e-401b-bc02-d992402d6bf4");

                report.Prepare();

                using (MemoryStream ms = new())
                {
                    PDFSimpleExport pdfExport = new PDFSimpleExport();
                    pdfExport.Export(report, ms);
                    ms.Flush();
                    return File(ms.ToArray(), "application/pdf", "SalesOrder.pdf");
                }
            }
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
            return File(ms.ToArray(), "application/pdf", "SalesOrder.pdf");
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

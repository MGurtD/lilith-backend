using Application.Services;
using FastReport;
using FastReport.Data;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IFileService _fileService;

        public ReportController(IFileService fileService)
        {
            _fileService = fileService;

            FastReport.Utils.RegisteredObjects.AddConnection(typeof(PostgresDataConnection));
        }

        [HttpGet]
        public IActionResult Get()
        {
            using (var report = new Report())
            {
                report.Load("C:\\Users\\mgurt\\Desktop\\OneDrive - ENGINYERIA MAPEX S.L\\Customers\\Temges\\Development\\Reports\\SalesOrder.frx");

                report.SetParameterValue("NYE", "1234");

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
        public async Task<IActionResult> Download(Domain.Entities.File file)
        {
            if (!System.IO.File.Exists(file.Path))
                return NotFound();

            var memory = new MemoryStream();
            await using (var stream = new FileStream(file.Path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, Services.FileService.GetContentType(file.Path), Path.GetFileName(file.Path));
        }
    }
}

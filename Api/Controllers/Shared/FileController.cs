using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController(IFileService fileService) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetEntityFiles(string entity, Guid entityId)
        {
            var files = fileService.GetEntityFiles(entity, entityId);
            return Ok(files);
        }

        [Route("Documents")]
        [HttpGet]
        public IActionResult GetEntityDocuments(string entity, Guid entityId)
        {
            var files = fileService.GetEntityDocuments(entity, entityId);
            return Ok(files);
        }

        [Route("Images")]
        [HttpGet]
        public IActionResult GetEntityImages(string entity, Guid entityId)
        {
            var files = fileService.GetEntityImages(entity, entityId);
            return Ok(files);
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
            
            var fileName = Path.GetFileName(file.Path);

            // CRÍTICO: Forzar descarga binaria para archivos CAD
            var ext = Path.GetExtension(file.Path).ToLowerInvariant();
            var contentType = ext switch
            {
                ".stp" or ".step" or ".dxf" => "application/octet-stream",
                _ => Services.FileService.GetContentType(file.Path)
            };
            
            // Headers críticos para Chrome Android
            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            Response.Headers.Append("Content-Type", contentType);
            Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            return File(memory, contentType, fileName);
        }

        [Route("Upload")]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string entity, [FromForm] string id)
        {
            var response = await fileService.UploadFile(file, entity, Guid.Parse(id));
            return response.Result ? Ok(response.Content) : BadRequest(response.Errors);
        }
        
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await fileService.RemoveFile(id);
            return response.Result ? Ok(response.Content) : BadRequest(response.Errors);
        }
    }
}

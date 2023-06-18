using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public IActionResult GetEntityFiles(string entity, Guid entityId)
        {
            var files = _fileService.GetEntityFiles(entity, entityId);
            return Ok(files);
        }

        [Route("Documents")]
        [HttpGet]
        public IActionResult GetEntityDocuments(string entity, Guid entityId)
        {
            var files = _fileService.GetEntityDocuments(entity, entityId);
            return Ok(files);
        }

        [Route("Images")]
        [HttpGet]
        public IActionResult GetEntityImages(string entity, Guid entityId)
        {
            var files = _fileService.GetEntityImages(entity, entityId);
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
            return File(memory, Services.FileService.GetContentType(file.Path), Path.GetFileName(file.Path));
        }

        [Route("Upload")]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string entity, [FromForm] string id)
        {
            var uploaded = await _fileService.UploadFile(file, entity, Guid.Parse(id));
            return uploaded ? Ok(uploaded) : BadRequest();
        }
        
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _fileService.RemoveFile(id);
            if (result)
                return Ok();
            else
                return BadRequest();
        }
    }
}

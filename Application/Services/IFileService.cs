using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public interface IFileService
    {
        IEnumerable<object> GetFiles();
        Task<bool> UploadFile(IFormFile file);

        Task<bool> UploadFiles(IEnumerable<IFormFile> file);
        Task<bool> RemoveFile(string filePath);
    }
}
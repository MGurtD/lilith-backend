using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public interface IFileService
    {
        IEnumerable<Domain.Entities.File> GetEntityFiles(string Entity, Guid EntityId);
        IEnumerable<Domain.Entities.File> GetEntityDocuments(string Entity, Guid EntityId);
        IEnumerable<Domain.Entities.File> GetEntityImages(string Entity, Guid EntityId);
        Task<bool> UploadFile(IFormFile file);
        Task<bool> UploadFiles(IEnumerable<IFormFile> file);
        Task<bool> RemoveEntityFiles(string Entity, Guid EntityId);
        Task<bool> RemoveFile(Guid id);
    }
}
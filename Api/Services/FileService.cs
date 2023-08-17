using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.StaticFiles;

namespace Api.Services
{
    public class FileService : IFileService
    {

        public readonly IUnitOfWork unitOfWork;

        public FileService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IEnumerable<object> GetFiles()
        {

            return Directory.GetFiles(Configuration.FileUploadPath);
        }     

        public async Task<GenericResponse> UploadFile(IFormFile file, string entity, Guid id)
        {
            try
            {
                if (file.Length > 0 && CreateDirectory(entity))
                {
                    // Crear arxiu al filesystem
                    var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
                    var fileName = $"{id}_{timestamp}{Path.GetExtension(file.FileName)}";
                    var path = Path.Combine(Configuration.FileUploadPath, entity, fileName);
                    using var fileStream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(fileStream);

                    // Crear arxiu a la BDD
                    var dbFile = new Domain.Entities.File()
                    {
                        Entity = entity,
                        EntityId = id,
                        Path = path,
                        OriginalName = file.FileName,
                        Size = file.Length,
                        Type = GetTypeFromFormFile(file)
                    };
                    await unitOfWork.Files.Add(dbFile);

                    return new GenericResponse(true, new List<string>(), dbFile);
                }
                else
                {
                    return new GenericResponse(false, new List<string>() { $"Problemes al crear el directori '${entity}'" });
                }
            }
            catch (Exception ex)
            {
                return new GenericResponse(false, new List<string>() { ex.Message });
            }
        }

        public IEnumerable<Domain.Entities.File> GetEntityFiles(string Entity, Guid EntityId)
        {
            var files = unitOfWork.Files.Find(f => f.Entity == Entity && f.EntityId == EntityId);
            return files;
        }

        public IEnumerable<Domain.Entities.File> GetEntityDocuments(string Entity, Guid EntityId)
        {
            var files = unitOfWork.Files.Find(f => f.Entity == Entity && f.EntityId == EntityId && f.Type == (int)FileType.Document);
            return files;
        }

        public IEnumerable<Domain.Entities.File> GetEntityImages(string Entity, Guid EntityId)
        {
            var files = unitOfWork.Files.Find(f => f.Entity == Entity && f.EntityId == EntityId && f.Type == FileType.Image);
            return files;
        }

        public async Task<GenericResponse> RemoveEntityFiles(string Entity, Guid EntityId)
        {
            var files = GetEntityFiles(Entity, EntityId);

            try
            {
                foreach (var file in files)
                {
                    System.IO.File.Delete(file.Path);
                }
                await unitOfWork.Files.RemoveRange(files);

                return new GenericResponse(false, new List<string>(), files);
            }
            catch (Exception ex)
            {
                return new GenericResponse(false, new List<string>() { ex.Message });
            }
        }

        public async Task<GenericResponse> RemoveFile(Guid id)
        {
            try
            {
                var file = await unitOfWork.Files.Get(id);
                if (file is not null)
                {
                    System.IO.File.Delete(file.Path);
                    await unitOfWork.Files.Remove(file);
                }

                return new GenericResponse(false, new List<string>(), file);
            }
            catch (Exception ex)
            {
                return new GenericResponse(false, new List<string>() { ex.Message });
            }
        }

        public static string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            var contentType = string.Empty;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private bool CreateDirectory(string entity)
        {
            try
            {
                string rootPath = Path.Combine(Configuration.FileUploadPath, entity);
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private FileType GetTypeFromFormFile(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                    return FileType.Image;
                case ".mp4":
                case ".mov":
                case ".wmv":
                case ".flv":
                    return FileType.Video;
                default:
                    return FileType.Document;
            }
        }
    }
}

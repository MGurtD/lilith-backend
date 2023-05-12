using Application.Services;

namespace Api.Services
{
    public class FileService : IFileService
    {
        public IEnumerable<object> GetFiles()
        {
            return Directory.GetFiles(ApplicationConfiguration.FileUploadPath);
        }        

        public async Task<bool> UploadFile(IFormFile file)
        {
            try
            {
                if (file.Length > 0)
                {
                    string? path = ApplicationConfiguration.FileUploadPath;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    using var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create);

                    await file.CopyToAsync(fileStream);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File Copy Failed", ex);
            }
        }

        public async Task<bool> UploadFiles(IEnumerable<IFormFile> files)
        {
            var uploaded = false;
            foreach (var file in files)
            {
                uploaded = await UploadFile(file);
            }
            return uploaded;
        }

        public Task<bool> RemoveFile(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}

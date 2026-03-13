using KBA.Framework.Storage.Interfaces;

namespace KBA.Framework.Storage.Providers;

public class LocalBlobStorageService : IBlobStorageService
{
    private readonly string _basePath;

    public LocalBlobStorageService(string basePath = "wwwroot/uploads")
    {
        _basePath = basePath;
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadAsync(string containerName, string fileName, Stream content)
    {
        var containerPath = Path.Combine(_basePath, containerName);
        if (!Directory.Exists(containerPath))
        {
            Directory.CreateDirectory(containerPath);
        }

        var filePath = Path.Combine(containerPath, fileName);
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream);

        return $"/uploads/{containerName}/{fileName}";
    }

    public async Task<Stream> DownloadAsync(string containerName, string fileName)
    {
        var filePath = Path.Combine(_basePath, containerName, fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File {fileName} not found in {containerName}");
        }

        // Return a stream that the caller must dispose
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public Task DeleteAsync(string containerName, string fileName)
    {
        var filePath = Path.Combine(_basePath, containerName, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}

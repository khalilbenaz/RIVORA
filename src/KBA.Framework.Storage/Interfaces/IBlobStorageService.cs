namespace KBA.Framework.Storage.Interfaces;
public interface IBlobStorageService
{
    Task<string> UploadAsync(string containerName, string fileName, Stream content);
    Task<Stream> DownloadAsync(string containerName, string fileName);
    Task DeleteAsync(string containerName, string fileName);
}

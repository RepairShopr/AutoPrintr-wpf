using System;
using System.Threading.Tasks;

namespace AutoPrintr.Core.IServices
{
    public interface IFileService
    {
        Task<string> ReadStringAsync(string fileName);
        Task<byte[]> ReadBytesAsync(string fileName);
        Task<T> ReadObjectAsync<T>(string fileName);
        Task SaveStringAsync(string fileName, string content);
        Task SaveBytesAsync(string fileName, byte[] content);
        Task SaveObjectAsync<T>(string fileName, T content);
        bool DeleteFile(string fileName);
        Task DownloadFileAsync(Uri fileUri, string localFilePath, Action<int> progressChanged = null, Action<bool, Exception> completed = null);
        string GetFilePath(string fileName);
        void SetupAccessControl(string fileName);
    }
}
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface IFileService
    {
        Task<string> ReadStringAsync(string fileName);
        Task<byte[]> ReadBytesAsync(string fileName);
        Task<T> ReadObjectAsync<T>(string fileName);
        Task SaveStringAsync(string fileName, string content);
        Task SaveBytesAsync(string fileName, byte[] content);
        Task SaveObjectAsync<T>(string fileName, T content);
        Task DeleteFileAsync(string fileName);
    }
}
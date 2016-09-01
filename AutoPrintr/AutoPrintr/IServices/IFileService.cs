using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface IFileService
    {
        Task<string> ReadStringAsync(string fileName);
        Task<byte[]> ReadBytesAsync(string fileName);
        Task<T> ReadObjectAsync<T>(string fileName);
        Task SaveFileAsync<T>(string fileName, T content);
        Task DeleteFileAsync(string fileName);
    }
}
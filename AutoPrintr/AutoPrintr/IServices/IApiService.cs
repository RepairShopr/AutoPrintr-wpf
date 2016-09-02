using AutoPrintr.Helpers;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface IApiService
    {
        Task<ApiResult<T>> LoginAsync<T>(string baseUrl, string url, string email, string password);
        Task<ApiResult<T>> GetAsync<T>(string baseUrl, string url);
        Task<ApiResult<T>> PostAsync<T>(string baseUrl, string url, object obj);
        Task<ApiResult<T>> PutAsync<T>(string baseUrl, string url, object obj);
        Task<ApiResult<bool>> DeleteAsync(string baseUrl, string url);
    }
}
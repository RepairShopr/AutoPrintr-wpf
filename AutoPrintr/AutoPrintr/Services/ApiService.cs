using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AutoPrintr.Services
{
    internal class ApiService : IApiService
    {
        #region Properties
        private const int TIMEOUT = 30;

        private readonly ILoggerService _loggingService;
        #endregion

        #region Constructors
        public ApiService(ILoggerService loggingService)
        {
            _loggingService = loggingService;
        }
        #endregion

        #region Methods
        public async Task<ApiResult<T>> LoginAsync<T>(string baseUrl, string url, string email, string password)
        {
            using (var client = CreateHttpClient(baseUrl))
            {
                try
                {
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(string.Concat(baseUrl, url));
                    var parms = new[]
                    {
                        new KeyValuePair<string, string>(nameof(email), email),
                        new KeyValuePair<string, string>(nameof(password), password)
                    };
                    request.Content = new FormUrlEncodedContent(parms);

                    var response = await client.SendAsync(request);
                    return await ReadResponseAsync<T>(response);
                }
                catch (HttpRequestException ex)
                {
                    return CreateHttpRequestExceptionResult<T>(ex);
                }
            }
        }

        public async Task<ApiResult<T>> GetAsync<T>(string baseUrl, string url)
        {
            using (var client = CreateHttpClient(baseUrl))
            {
                try
                {
                    var response = await client.GetAsync(url);
                    return await ReadResponseAsync<T>(response);
                }
                catch (HttpRequestException ex)
                {
                    return CreateHttpRequestExceptionResult<T>(ex);
                }
            }
        }

        public async Task<ApiResult<T>> PostAsync<T>(string baseUrl, string url, object obj)
        {
            using (var client = CreateHttpClient(baseUrl))
            {
                try
                {
                    var response = await client.PostAsJsonAsync(url, obj);
                    return await ReadResponseAsync<T>(response);
                }
                catch (HttpRequestException ex)
                {
                    return CreateHttpRequestExceptionResult<T>(ex);
                }
            }
        }

        public async Task<ApiResult<T>> PutAsync<T>(string baseUrl, string url, object obj)
        {
            using (var client = CreateHttpClient(baseUrl))
            {
                try
                {
                    var response = await client.PutAsJsonAsync(url, obj);
                    return await ReadResponseAsync<T>(response);
                }
                catch (HttpRequestException ex)
                {
                    return CreateHttpRequestExceptionResult<T>(ex);
                }
            }
        }

        public async Task<ApiResult<bool>> DeleteAsync(string baseUrl, string url)
        {
            using (var client = CreateHttpClient(baseUrl))
            {
                try
                {
                    var response = await client.DeleteAsync(url);
                    return new ApiResult<bool>
                    {
                        IsSuccess = response.IsSuccessStatusCode,
                        Result = response.IsSuccessStatusCode
                    };
                }
                catch (HttpRequestException ex)
                {
                    return CreateHttpRequestExceptionResult<bool>(ex);
                }
            }
        }

        private async Task<ApiResult<T>> ReadResponseAsync<T>(HttpResponseMessage response)
        {
            var result = new ApiResult<T>();
            result.IsSuccess = response.IsSuccessStatusCode;
            result.StatusCode = response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Error with code #{response.StatusCode} in {nameof(ApiService.ReadResponseAsync)}: {message}");
                _loggingService.WriteError(message);
            }
            else
                result.Result = await response.Content.ReadAsAsync<T>();

            return result;
        }

        private ApiResult<T> CreateHttpRequestExceptionResult<T>(HttpRequestException ex)
        {
            Debug.WriteLine($"Error in {nameof(ApiService)}: {ex.ToString()}");
            _loggingService.WriteError(ex);

            return new ApiResult<T>
            {
                IsSuccess = false,
                StatusCode = System.Net.HttpStatusCode.RequestTimeout
            };
        }

        private HttpClient CreateHttpClient(string baseUrl)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(TIMEOUT);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
        #endregion
    }
}
using AutoPrintr.Core.Helpers;
using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.Core.Services
{
    internal class UserService : IUserService
    {
        #region Properties
        private readonly IEnumerable<string> _hostNames;
        private readonly IApiService _apiService;
        private readonly ILoggerService _loggingService;
        #endregion

        #region Constructors
        public UserService(IAppSettings appSettings,
            IApiService apiService,
            ILoggerService loggingService)
        {
            _apiService = apiService;
            _loggingService = loggingService;
            _hostNames = appSettings.HostNames;
        }
        #endregion

        #region Methods
        public async Task<User> LoginAsync(Login login)
        {
            foreach (var hostName in _hostNames)
            {
                _loggingService.WriteInformation($"Authenticating user {login.Username} with {hostName}");

                var result = await TryLoginAsync(login, hostName);
                if (result.IsSuccess)
                {
                    _loggingService.WriteInformation($"User {login.Username} is authenticated with {hostName}");
                    _loggingService.User = result.Result;
                    _loggingService.User.Domain = hostName;

                    return result.Result;
                }
                else
                    _loggingService.WriteInformation($"User {login.Username} is not authenticated with {hostName}");
            }

            return null;
        }

        public async Task<Channel> GetChannelAsync(User user)
        {
            var baseUrl = $"https://{user.Subdomain}.{user.Domain}/api/v1/";
            var action = $"settings/printing?api_key={user.Token}";

            var result = await _apiService.GetAsync<Channel>(baseUrl, action);
            return result.Result;
        }

        private async Task<ApiResult<User>> TryLoginAsync(Login login, string hostName)
        {
            var baseUrl = $"https://admin.{hostName}/api/v1/";
            var action = "sign_in";

            return await _apiService.LoginAsync<User>(baseUrl, action, login.Username, login.Password);
        }
        #endregion
    }
}
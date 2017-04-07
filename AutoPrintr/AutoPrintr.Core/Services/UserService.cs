using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System.Threading.Tasks;

namespace AutoPrintr.Core.Services
{
    internal class UserService : IUserService
    {
        #region Properties
        private readonly string _hostName;
        private readonly IApiService _apiService;
        private readonly ILoggerService _loggingService;
        #endregion

        #region Constructors
        public UserService(IApiService apiService,
            ILoggerService loggingService)
        {
            _apiService = apiService;
            _loggingService = loggingService;
            _hostName = "repairshopr.com";
        }
        #endregion

        #region Methods
        public async Task<User> LoginAsync(Login login)
        {
            _loggingService.WriteInformation($"Authenticating user {login.Username}");

            var baseUrl = $"https://admin.{_hostName}/api/v1/";
            var action = "sign_in";

            var result = await _apiService.LoginAsync<User>(baseUrl, action, login.Username, login.Password);

            if (result.IsSuccess)
                _loggingService.WriteInformation($"User {login.Username} is authenticated");
            else
                _loggingService.WriteInformation($"User {login.Username} is not authenticated");
            
            return result.Result;
        }

        public async Task<Channel> GetChannelAsync(User user)
        {
            var baseUrl = $"https://{user.Subdomain}.{_hostName}/api/v1/";
            var action = $"settings/printing?api_key={user.Token}";

            var result = await _apiService.GetAsync<Channel>(baseUrl, action);
            return result.Result;
        }
        #endregion
    }
}
using AutoPrintr.IServices;
using AutoPrintr.Models;
using System.Threading.Tasks;

namespace AutoPrintr.Services
{
    internal class UserService : IUserService
    {
        #region Properties
        private readonly string _hostName;
        private readonly IApiService _apiService;
        #endregion

        #region Constructors
        public UserService(IApiService apiService)
        {
            _apiService = apiService;
            _hostName = "repairshopr.com";
        }
        #endregion

        #region Methods
        public async Task<User> LoginAsync(Login login)
        {
            var baseUrl = $"https://admin.{_hostName}/api/v1/";
            var action = "sign_in";

            var result = await _apiService.LoginAsync<User>(baseUrl, action, login.Username, login.Password);
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
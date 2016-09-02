using AutoPrintr.IServices;
using AutoPrintr.Models;
using System.Threading.Tasks;

namespace AutoPrintr.Services
{
    internal class UserService : IUserService
    {
        #region Properties
        //TODO: Move host name into App.config file and inject in constructor
        private const string HOST_NAME = "repairshopr.com";
        private readonly IApiService _apiService;
        #endregion

        #region Constructors
        public UserService(IApiService apiService)
        {
            _apiService = apiService;
        }
        #endregion

        #region Methods
        public async Task<User> LoginAsync(Login login)
        {
            var baseUrl = $"https://admin.{HOST_NAME}/api/v1/";
            var action = "sign_in";

            var result = await _apiService.LoginAsync<User>(baseUrl, action, login.Username, login.Password);
            return result.Result;
        }

        public async Task<Channel> GetChannelAsync(User user)
        {
            var baseUrl = $"https://{user.Subdomain}.{HOST_NAME}/api/v1/";
            var action = $"settings/printing?api_key={user.Token}";

            var result = await _apiService.GetAsync<Channel>(baseUrl, action);
            return result.Result;
        }
        #endregion
    }
}
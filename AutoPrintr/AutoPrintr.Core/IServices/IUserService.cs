using AutoPrintr.Core.Models;
using System.Threading.Tasks;

namespace AutoPrintr.Core.IServices
{
    public interface IUserService
    {
        Task<User> LoginAsync(Login login);
        Task<Channel> GetChannelAsync(User user);
    }
}
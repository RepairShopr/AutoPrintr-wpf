using AutoPrintr.Models;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface IUserService
    {
        Task<User> LoginAsync(Login login);
        Task<Channel> GetChannelAsync(User user);
    }
}
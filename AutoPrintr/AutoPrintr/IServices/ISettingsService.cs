using AutoPrintr.Models;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface ISettingsService
    {
        Settings Settings { get; }
        Task LoadSettingsAsync();
        Task SaveSettingsAsync();
    }
}
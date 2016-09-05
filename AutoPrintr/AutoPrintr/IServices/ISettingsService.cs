using AutoPrintr.Models;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface ISettingsService
    {
        Settings Settings { get; }
        Task LoadSettingsAsync();
        Task SetSettingsAsync(User user, Channel channel = null);
        Task AddLocationAsync(Location location);
        Task RemoveLocationAsync(Location location);
        Task AddPrinterAsync(Printer printer);
        Task UpdatePrinterAsync(Printer printer);
        Task RemovePrinterAsync(Printer printer);
    }
}
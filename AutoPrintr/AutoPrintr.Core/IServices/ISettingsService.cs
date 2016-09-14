using AutoPrintr.Core.Models;
using System.Threading.Tasks;

namespace AutoPrintr.Core.IServices
{
    public delegate void ChannelChangedEventHandler(Channel newChannel);

    public interface ISettingsService
    {
        event ChannelChangedEventHandler ChannelChangedEvent;

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
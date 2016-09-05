using AutoPrintr.IServices;
using PusherClient;
using System;
using System.Threading.Tasks;

namespace AutoPrintr.Services
{
    internal class JobsService : IJobsService
    {
        #region Properties
        private readonly ISettingsService _settingsService;
        private readonly string _applicationKey;

        private string _channel;
        private Pusher _pusher;
        #endregion

        #region Constructors
        public JobsService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _applicationKey = "";

            _settingsService.ChannelChangedEvent += _settingsService_ChannelChangedEvent;
        }
        #endregion

        #region Methods
        public async Task RunAsync()
        {
            if (_settingsService.Settings.Channel == null || string.IsNullOrEmpty(_settingsService.Settings.Channel.Value))
            {
                _channel = null;
                await StopPusher();
                return;
            }

            if (string.Compare(_channel, _settingsService.Settings.Channel.Value, true) != 0)
            {
                _channel = _settingsService.Settings.Channel.Value;
                await StopPusher();
                await StartPusher();
            }
        }

        private async Task StartPusher()
        {
            await Task.Factory.StartNew(() =>
            {
                if (_pusher == null)
                    return;

                _pusher = new Pusher(_applicationKey);
                _pusher.Error += _pusher_Error;
                _pusher.Subscribe(_channel)
                       .Bind("print-job", _pusher_ReadResponse);

                _pusher.Connect();
            });
        }

        private async Task StopPusher()
        {
            await Task.Factory.StartNew(() =>
            {
                if (_pusher == null)
                    return;

                _pusher.Disconnect();
                _pusher = null;
            });
        }

        private void _pusher_ReadResponse(dynamic message)
        {

        }

        private void _pusher_Error(object sender, PusherException error)
        {
            throw new NotImplementedException();
        }

        private async void _settingsService_ChannelChangedEvent(Models.Channel newChannel)
        {
            await RunAsync();
        }
        #endregion
    }
}
using AutoPrintr.Core.IServices;
using System.Collections.Generic;

namespace AutoPrintr.Core.Services
{
    internal class AppSettings : IAppSettings
    {
        public string PusherApplicationKey =>
            "";
        public string RollbarAccessToken =>
            "";
        public IEnumerable<string> HostNames =>
            new[]
            {
                "repairshopr.com",
                "syncromsp.com"
            };
    }
}
using System.Collections.Generic;

namespace AutoPrintr.Core.IServices
{
    public interface IAppSettings
    {
        string PusherApplicationKey { get; }
        string RollbarAccessToken { get; }
        IEnumerable<string> HostNames { get; }
    }
}
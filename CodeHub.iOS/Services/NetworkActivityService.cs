using CodeHub.Core.Services;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.Services
{
    public class NetworkActivityService : INetworkActivityService
    {
        public void PopNetworkActive()
        {
            NetworkActivity.PopNetworkActive();
        }

        public void PushNetworkActive()
        {
            NetworkActivity.PushNetworkActive();
        }
    }
}

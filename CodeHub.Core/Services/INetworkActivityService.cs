using System;

namespace CodeHub.Core.Services
{
    public interface INetworkActivityService
    {
        void PushNetworkActive();

        void PopNetworkActive();
    }
}

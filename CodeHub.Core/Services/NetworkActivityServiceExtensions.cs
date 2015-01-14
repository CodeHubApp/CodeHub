using System;
using System.Reactive.Disposables;

namespace CodeHub.Core.Services
{
    public static class NetworkActivityServiceExtensions
    {
        public static IDisposable ActivateNetwork(this INetworkActivityService @this)
        {
            @this.PushNetworkActive();
            return Disposable.Create(@this.PopNetworkActive);
        }
    }
}


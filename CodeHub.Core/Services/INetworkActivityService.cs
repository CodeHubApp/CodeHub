using System;
using System.Reactive.Disposables;

namespace CodeHub.Core.Services
{
    public interface INetworkActivityService
    {
        void PushNetworkActive();

        void PopNetworkActive();
    }

    public static class NetworkActivityServiceExtensions
    {
        public static IDisposable ActivateNetwork(this INetworkActivityService @this)
        {
            @this.PushNetworkActive();
            return Disposable.Create(@this.PopNetworkActive);
        }
    }
}

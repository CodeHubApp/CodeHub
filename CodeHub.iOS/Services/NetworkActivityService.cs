using UIKit;
using System;
using System.Reactive.Disposables;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Services
{
    public class NetworkActivityService : INetworkActivityService
    {
        private static readonly Lazy<NetworkActivityService> _instance = new Lazy<NetworkActivityService>(() => new NetworkActivityService());
        private static UIApplication MainApp = UIApplication.SharedApplication;
        static readonly object NetworkLock = new object();
        static int _active;

        public static NetworkActivityService Instance
        {
            get { return _instance.Value; }
        }

        private NetworkActivityService()
        {
        }

        public void PushNetworkActive()
        {
            lock (NetworkLock)
            {
                _active++;
                MainApp.NetworkActivityIndicatorVisible = true;
            }
        }

        public void PopNetworkActive()
        {
            lock (NetworkLock)
            {
                if (_active == 0)
                    return;

                _active--;
                if (_active == 0)
                    MainApp.NetworkActivityIndicatorVisible = false;
            }
        }

        public IDisposable ActivateNetwork()
        {
            PushNetworkActive();
            return Disposable.Create(PopNetworkActive);
        }
    }
}
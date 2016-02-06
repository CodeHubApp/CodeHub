using CodeHub.Core.ViewModels.App;
using CodeHub.Core.Services;
using System.Net;
using System.Net.Http;
using ModernHttpClient;
using System.Threading.Tasks;
using System;
using Foundation;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core
{
    /// <summary>
    /// Define the App type.
    /// </summary>
    public class App : MvxApplication
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

			//Ensure this is loaded
            MvvmCross.Plugins.Messenger.PluginLoader.Instance.EnsureLoaded();

            GitHubSharp.Client.ClientConstructor = () => new HttpClient(new HttpMessageHandler());

            // Start the app with the First View Model.
			this.RegisterAppStart<StartupViewModel>();
        }
    }

    class HttpMessageHandler : NativeMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (!string.Equals(request.Method.ToString(), "get", StringComparison.OrdinalIgnoreCase))
                NSUrlCache.SharedCache.RemoveAllCachedResponses();
            return base.SendAsync(request, cancellationToken);
        }
    }

}
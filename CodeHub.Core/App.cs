using CodeHub.Core.ViewModels.App;
using System.Net;
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
        }
    }
}
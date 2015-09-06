using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.ViewModels.App;
using CodeFramework.Core.Services;
using Cirrious.CrossCore;

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
			//Ensure this is loaded
			Cirrious.MvvmCross.Plugins.Messenger.PluginLoader.Instance.EnsureLoaded();

			var httpService = Mvx.Resolve<IHttpClientService>();
			GitHubSharp.Client.ClientConstructor = httpService.Create;

            // Start the app with the First View Model.
			this.RegisterAppStart<StartupViewModel>();
        }
    }
}
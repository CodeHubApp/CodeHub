using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.ViewModels;
using CodeHub.Core.ViewModels.App;

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

            // Start the app with the First View Model.
			this.RegisterAppStart<StartupViewModel>();
        }
    }
}
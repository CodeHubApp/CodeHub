// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the AppDelegate type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Cirrious.MvvmCross.Views;
using CodeFramework.Core.Data;
using CodeFramework.Core.Services;
using CodeFramework.Core.ViewModels;
using CodeFramework.iOS;
using CodeFramework.iOS.ViewControllers;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Views;

namespace CodeHub.iOS
{
    using Cirrious.CrossCore;
    using Cirrious.MvvmCross.Touch.Platform;
    using Cirrious.MvvmCross.Touch.Views.Presenters;
    using Cirrious.MvvmCross.ViewModels;
    using MonoTouch.Foundation;
    using MonoTouch.UIKit;

    /// <summary>
    /// The UIApplicationDelegate for the application. This class is responsible for launching the 
    /// User Interface of the application, as well as listening (and optionally responding) to 
    /// application events from iOS.
    /// </summary>
    [Register("AppDelegate")]
    public class AppDelegate : MvxApplicationDelegate
    {
        /// <summary>
        /// The window.
        /// </summary>
        private UIWindow window;

        /// <summary>
        /// Finished the launching.
        /// </summary>
        /// <param name="app">The app.</param>
        /// <param name="options">The options.</param>
        /// <returns>True or false.</returns>
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            this.window = new UIWindow(UIScreen.MainScreen.Bounds);

            // Setup theme
            Theme.Setup();

            var presenter = new TouchViewPresenter(this.window);

            var setup = new Setup(this, presenter);
            setup.Initialize();

            IAccountsService<IAccount> a = new GitHubAccountsService(null, null);

            var accountsService = Mvx.Resolve<IAccountsService<GitHubAccount>>();
            Mvx.RegisterSingleton(typeof(IAccountsService<IAccount>), accountsService);

            var startup = Mvx.Resolve<IMvxAppStart>();
            startup.Start();

            this.window.MakeKeyAndVisible();

            return true;
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the AppDelegate type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CodeFramework.iOS;

namespace CodeHub.iOS
{
    using Cirrious.CrossCore;
    using Cirrious.MvvmCross.Touch.Platform;
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
			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
			UINavigationBar.Appearance.TintColor = UIColor.White;
			UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(39, 41, 43);
			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes { TextColor = UIColor.White });
			UISegmentedControl.Appearance.TintColor = UIColor.FromRGB(120, 120, 127);
			//UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).TintColor = UIColor.FromRGB(120, 120, 127);

			this.window = new UIWindow(UIScreen.MainScreen.Bounds);


            // Setup theme
            Theme.Setup();

            var presenter = new TouchViewPresenter(this.window);

            var setup = new Setup(this, presenter);
            setup.Initialize();

            var startup = Mvx.Resolve<IMvxAppStart>();
			startup.Start();

            this.window.MakeKeyAndVisible();

            return true;
        }
    }
}
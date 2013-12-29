// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the AppDelegate type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CodeFramework.iOS;
using System.Collections.Generic;
using System;    
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Touch.Platform;
using Cirrious.MvvmCross.ViewModels;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CodeHub.iOS
{
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

		public string DeviceToken;

		/// <summary>
		/// This is the main entry point of the application.
		/// </summary>
		/// <param name="args">The args.</param>
		public static void Main(string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, "AppDelegate");
		}

		private class UserVoiceStyleSheet : UserVoice.UVStyleSheet
		{
			public override UIColor NavigationBarTextColor
			{
				get
				{
					return UIColor.White;
				}
			}

			public override UIColor NavigationBarTintColor
			{
				get
				{
					return UIColor.White;
				}
			}
		}

        /// <summary>
        /// Finished the launching.
        /// </summary>
        /// <param name="app">The app.</param>
        /// <param name="options">The options.</param>
        /// <returns>True or false.</returns>
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
			var iRate = MTiRate.iRate.SharedInstance;
			iRate.AppStoreID = 707173885;

			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
			UINavigationBar.Appearance.TintColor = UIColor.White;
			UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(50, 50, 50);
			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(18f) });
			CodeFramework.iOS.Utils.Hud.BackgroundTint = UIColor.FromRGBA(228, 228, 228, 128);

			UserVoice.UVStyleSheet.StyleSheet = new UserVoiceStyleSheet();

			UISegmentedControl.Appearance.TintColor = UIColor.FromRGB(110, 110, 117);
			UITableViewHeaderFooterView.Appearance.TintColor = UIColor.FromRGB(228, 228, 228);
			UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).TextColor = UIColor.FromRGB(136, 136, 136);
			UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).Font = UIFont.SystemFontOfSize(13f);

			UIToolbar.Appearance.BarTintColor = UIColor.FromRGB(245, 245, 245);

			UIBarButtonItem.AppearanceWhenContainedIn(typeof(UISearchBar)).SetTitleTextAttributes(new UITextAttributes()
			{
				TextColor = UIColor.White,
			}, UIControlState.Normal);

			this.window = new UIWindow(UIScreen.MainScreen.Bounds);

            // Setup theme
            Theme.Setup();

            var presenter = new TouchViewPresenter(this.window);

            var setup = new Setup(this, presenter);
            setup.Initialize();

			Mvx.Resolve<CodeFramework.Core.Services.IAnalyticsService>().Init("UA-44040302-1", "CodeHub");

            var startup = Mvx.Resolve<IMvxAppStart>();
			startup.Start();

            this.window.MakeKeyAndVisible();

			if (options != null)
			{
				if (options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey)) 
				{
					var remoteNotification = options[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
					if(remoteNotification != null) {
						HandleNotification(remoteNotification);
					}
				}
			}

			// Notifications don't work on teh simulator so don't bother
			if (MonoTouch.ObjCRuntime.Runtime.Arch != MonoTouch.ObjCRuntime.Arch.SIMULATOR)
			{
				const UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge;
				UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
			}

            return true;
        }

		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, System.Action<UIBackgroundFetchResult> completionHandler)
		{
			if (application.ApplicationState == UIApplicationState.Active)
				return;
			HandleNotification(userInfo);
		}

		private void HandleNotification(NSDictionary data)
		{
			try
			{
				var viewDispatcher = Mvx.Resolve<Cirrious.MvvmCross.Views.IMvxViewDispatcher>();
				var request = MvxViewModelRequest<CodeHub.Core.ViewModels.Repositories.RepositoryViewModel>.GetDefaultRequest();
				var repoId = new CodeHub.Core.Utils.RepositoryIdentifier(data["r"].ToString());
				request.ParameterValues = new Dictionary<string, string>() {{"Username", repoId.Owner}, {"Repository", repoId.Name}};
				viewDispatcher.ShowViewModel(request);
			}
			catch (Exception e)
			{
				Console.WriteLine("Handle Notifications issue: " + e);
			}
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			DeviceToken = deviceToken.Description.Trim('<', '>').Replace(" ", "");
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			MonoTouch.Utilities.ShowAlert("Error Registering for Notifications", error.LocalizedDescription);
		}
    }
}
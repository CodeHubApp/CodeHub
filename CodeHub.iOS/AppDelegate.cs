using System;
using Foundation;
using UIKit;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using ReactiveUI;
using CodeHub.iOS.Views.App;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.App;
using Splat;
using CodeHub.iOS.Services;
using CodeHub.Core.Utilities;
using CodeHub.iOS.Views.Settings;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Factories;
using CodeHub.Core.Factories;
using CodeHub.Core.ViewModels.Changesets;
using System.Linq;
using CodeHub.Core;

namespace CodeHub.iOS
{
    /// <summary>
    /// The UIApplicationDelegate for the application. This class is responsible for launching the 
    /// User Interface of the application, as well as listening (and optionally responding) to 
    /// application events from iOS.
    /// </summary>
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public string DeviceToken;

        /// <summary>
        /// The window.
        /// </summary>
        public override UIWindow Window { get; set; }

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

        /// <summary>
        /// Finished the launching.
        /// </summary>
        /// <param name="app">The app.</param>
        /// <param name="options">The options.</param>
        /// <returns>True or false.</returns>
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            #if DEBUG
            Locator.CurrentMutable.Register(() => new DiagnosticLogger(), typeof(ILogger));
            #endif 

            // Stamp the date this was installed (first run)
            this.StampInstallDate("CodeHub", DateTime.Now.ToString());

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            Locator.CurrentMutable.InitializeFactories();
            Locator.CurrentMutable.InitializeServices();
            CodeHub.Core.Bootstrap.Init();
            Locator.Current.GetService<IErrorService>().Init("http://sentry.dillonbuchanan.com/api/5/store/", "17e8a650e8cc44678d1bf40c9d86529b ", "9498e93bcdd046d8bb85d4755ca9d330");


            var viewModelViews = Locator.Current.GetService<IViewModelViewService>();
            viewModelViews.RegisterViewModels(typeof(SettingsView).Assembly);

            Themes.Theme.Load("Default");

            var serviceConstructor = Locator.Current.GetService<IServiceConstructor>();
            var vm = serviceConstructor.Construct<StartupViewModel>();
            var startupViewController = new StartupView { ViewModel = vm };

            var mainNavigationController = new UINavigationController(startupViewController) { NavigationBarHidden = true };
            MessageBus.Current.Listen<LogoutMessage>().Subscribe(_ =>
            {
                mainNavigationController.PopToRootViewController(false);
                mainNavigationController.DismissViewController(true, null);
            });

            Window = new UIWindow(UIScreen.MainScreen.Bounds) {RootViewController = mainNavigationController};
            Window.MakeKeyAndVisible();

            SetupPushNotifications();
            HandleNotificationOptions(options);

            return true;
        }


        private void HandleNotificationOptions(NSDictionary options)
        {
            if (options == null) return;
            if (!options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey)) return;

            var remoteNotification = options[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
            remoteNotification.Do(x => HandleNotification(x, true));
        }

        private void SetupPushNotifications()
        {
            var features = Locator.Current.GetService<IFeaturesService>();

            // Automatic activations in debug mode!
#if DEBUG
            Locator.Current.GetService<IDefaultValueService>().Set(FeatureIds.PushNotifications, true);
#endif

            // Notifications don't work on teh simulator so don't bother
            if (ObjCRuntime.Runtime.Arch != ObjCRuntime.Arch.SIMULATOR && features.IsPushNotificationsActivated)
            {
                const UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
            }
        }

        // TODO: IMPORTANT!!!
//        void HandlePurchaseSuccess (object sender, string e)
//        {
//            IoC.Resolve<IDefaultValueService>().Set(e, true);
//
//            if (string.Equals(e, FeatureIds.PushNotifications))
//            {
//                const UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge;
//                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
//            }
//        }


		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, System.Action<UIBackgroundFetchResult> completionHandler)
		{
			if (application.ApplicationState == UIApplicationState.Active)
				return;
            HandleNotification(userInfo, false);
		}

        private static void HandleNotification(NSDictionary data, bool fromBootup)
		{
            var cmd = new PushNotificationCommand(data.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString()));
            Locator.Current.GetService<ISessionService>().SetStartupCommand(cmd);
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
            DeviceToken = deviceToken.Description.Trim('<', '>').Replace(" ", "");
            Locator.Current.GetService<ISessionService>().RegisterForNotifications();
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
            Locator.Current.GetService<IAlertDialogFactory>().Alert("Error Registering for Notifications", error.LocalizedDescription);
		}

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            var uri = new Uri(url.AbsoluteString);

            if (uri.Host == "x-callback-url")
            {
                //XCallbackProvider.Handle(new XCallbackQuery(url.AbsoluteString));
                return true;
            }
            else
            {
                var path = url.AbsoluteString.Replace("codehub://", "");
                var queryMarker = path.IndexOf("?", StringComparison.Ordinal);
                if (queryMarker > 0)
                    path = path.Substring(0, queryMarker);

                if (!path.EndsWith("/", StringComparison.Ordinal))
                    path += "/";
                var first = path.Substring(0, path.IndexOf("/", StringComparison.Ordinal));
                var firstIsDomain = first.Contains(".");

                var viewModel = Locator.Current.GetService<IUrlRouterService>().Handle(path);
                //TODO: Show the ViewModel
                return true;
            }
        }
    }
}
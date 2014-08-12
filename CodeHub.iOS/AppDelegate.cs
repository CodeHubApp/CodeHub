using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using MonoTouch.Security;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using CodeHub.iOS.Views.App;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.App;

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
			var iRate = MTiRate.iRate.SharedInstance;
			iRate.AppStoreID = 707173885;

            // Stamp the date this was installed (first run)
            this.StampInstallDate("CodeHub", DateTime.Now.ToString());

            // Load the IoC
            IoC.RegisterAssemblyServicesAsSingletons(typeof(Xamarin.Utilities.Core.Services.IDefaultValueService).Assembly);
            IoC.RegisterAssemblyServicesAsSingletons(typeof(Xamarin.Utilities.Services.DefaultValueService).Assembly);
            IoC.RegisterAssemblyServicesAsSingletons(typeof(Core.Services.IApplicationService).Assembly);
            IoC.RegisterAssemblyServicesAsSingletons(GetType().Assembly);

            var viewModelViewService = IoC.Resolve<IViewModelViewService>();
            viewModelViewService.RegisterViewModels(typeof(Xamarin.Utilities.Services.DefaultValueService).Assembly);
            viewModelViewService.RegisterViewModels(GetType().Assembly);

            IoC.Resolve<IErrorService>().Init("http://sentry.dillonbuchanan.com/api/5/store/", "17e8a650e8cc44678d1bf40c9d86529b ", "9498e93bcdd046d8bb85d4755ca9d330");
            CodeHub.Core.Bootstrap.Init();


            Theme.Setup();
            SetupPushNotifications();
            HandleNotificationOptions(options);

            var startupViewController = new StartupView { ViewModel = IoC.Resolve<StartupViewModel>() };
            startupViewController.ViewModel.View = startupViewController;

            var mainNavigationController = new UINavigationController(startupViewController) { NavigationBarHidden = true };
            MessageBus.Current.Listen<LogoutMessage>().Subscribe(_ =>
            {
                mainNavigationController.PopToRootViewController(false);
                mainNavigationController.DismissViewController(true, null);
            });

            Window = new UIWindow(UIScreen.MainScreen.Bounds) {RootViewController = mainNavigationController};
            Window.MakeKeyAndVisible();
            return true;
        }

        private void HandleNotificationOptions(NSDictionary options)
        {
            if (options == null) return;
            if (!options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey)) return;

            var remoteNotification = options[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
            if (remoteNotification != null)
            {
                HandleNotification(remoteNotification, true);
            }
        }

        private void SetupPushNotifications()
        {
            var features = IoC.Resolve<IFeaturesService>();

            // Automatic activations in debug mode!
#if DEBUG
            IoC.Resolve<IDefaultValueService>().Set(FeatureIds.PushNotifications, true);
#endif

            // Notifications don't work on teh simulator so don't bother
            if (MonoTouch.ObjCRuntime.Runtime.Arch != MonoTouch.ObjCRuntime.Arch.SIMULATOR && features.IsPushNotificationsActivated)
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

        private void HandleNotification(NSDictionary data, bool fromBootup)
		{
			try
			{
//				var viewDispatcher = Mvx.Resolve<Cirrious.MvvmCross.Views.IMvxViewDispatcher>();
//                var appService = Mvx.Resolve<IApplicationService>();
//                var repoId = new RepositoryIdentifier(data["r"].ToString());
//                var parameters = new Dictionary<string, string>() {{"Username", repoId.Owner}, {"Repository", repoId.Name}};
//
//                MvxViewModelRequest request;
//                if (data.ContainsKey(new NSString("c")))
//                {
//                    request = MvxViewModelRequest<CodeHub.Core.ViewModels.Changesets.ChangesetViewModel>.GetDefaultRequest();
//                    parameters.Add("Node", data["c"].ToString());
//                    parameters.Add("ShowRepository", "True");
//                }
//                else if (data.ContainsKey(new NSString("i")))
//                {
//                    request = MvxViewModelRequest<CodeHub.Core.ViewModels.Issues.IssueViewModel>.GetDefaultRequest();
//                    parameters.Add("Id", data["i"].ToString());
//                }
//                else if (data.ContainsKey(new NSString("p")))
//                {
//                    request = MvxViewModelRequest<CodeHub.Core.ViewModels.PullRequests.PullRequestViewModel>.GetDefaultRequest();
//                    parameters.Add("Id", data["p"].ToString());
//                }
//                else
//                {
//                    request = MvxViewModelRequest<CodeHub.Core.ViewModels.Repositories.RepositoryViewModel>.GetDefaultRequest();
//                }
//
//                request.ParameterValues = parameters;
//
//                var username = data["u"].ToString();
//
//                if (appService.Account == null || !appService.Account.Username.Equals(username))
//                {
//                    var user = appService.Accounts.FirstOrDefault(x => x.Username.Equals(username));
//                    if (user != null)
//                    {
//                        appService.DeactivateUser();
//                        appService.Accounts.SetDefault(user);
//                    }
//                }
//
//                appService.SetUserActivationAction(() => viewDispatcher.ShowViewModel(request));
//
//                if (appService.Account == null && !fromBootup)
//                {
//                    var startupViewModelRequest = MvxViewModelRequest<CodeHub.Core.ViewModels.App.StartupViewModel>.GetDefaultRequest();
//                    viewDispatcher.ShowViewModel(startupViewModelRequest);
//                }
			}
			catch (Exception e)
			{
				Console.WriteLine("Handle Notifications issue: " + e);
			}
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
            DeviceToken = deviceToken.Description.Trim('<', '>').Replace(" ", "");

            var app = IoC.Resolve<IApplicationService>();
            var accounts = IoC.Resolve<IAccountsService>();
            if (app.Account != null && !app.Account.IsPushNotificationsEnabled.HasValue)
            {
                Task.Run(() => IoC.Resolve<IPushNotificationsService>().Register());
                app.Account.IsPushNotificationsEnabled = true;
                accounts.Update(app.Account);
            }
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
            IoC.Resolve<IAlertDialogService>().Alert("Error Registering for Notifications", error.LocalizedDescription);
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

                return UrlRouteProvider.Handle(path);
            }
        }
    }
}
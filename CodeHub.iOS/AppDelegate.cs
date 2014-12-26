using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using ReactiveUI;
using CodeHub.iOS.Views.App;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.App;
using Splat;
using Xamarin.Utilities.Services;
using Xamarin.Utilities;
using Xamarin.Utilities.Factories;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Views;
using CodeHub.iOS.Services;
using CodeHub.Core.Utilities;
using CodeHub.iOS.Views.Settings;

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

			var iRate = MTiRate.iRate.SharedInstance;
			iRate.AppStoreID = 707173885;

            // Stamp the date this was installed (first run)
            this.StampInstallDate("CodeHub", DateTime.Now.ToString());

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            Locator.CurrentMutable.InitializeXamarinUtilities();
            Locator.CurrentMutable.InitializeLocal();
            CodeHub.Core.Bootstrap.Init();
            Locator.Current.GetService<IErrorService>().Init("http://sentry.dillonbuchanan.com/api/5/store/", "17e8a650e8cc44678d1bf40c9d86529b ", "9498e93bcdd046d8bb85d4755ca9d330");

            SetupPushNotifications();
            HandleNotificationOptions(options);

            var viewModelViews = Locator.Current.GetService<IViewModelViewService>();
            viewModelViews.RegisterViewModels(typeof(SettingsView).Assembly);
            viewModelViews.RegisterViewModels(typeof(WebBrowserView).Assembly);

            var accountsService = Locator.Current.GetService<IAccountsService>();
            accountsService.ActiveAccountChanged.Subscribe(x =>
            {
                try
                {
                    Themes.Theme.Load("Default");
                }
                catch
                {
                    Console.WriteLine("Crap!");
                }
            });

            var transitionOrchestration = Locator.Current.GetService<ITransitionOrchestrationService>();
            var serviceConstructor = Locator.Current.GetService<IServiceConstructor>();
            var vm = serviceConstructor.Construct<StartupViewModel>();
            var startupViewController = new StartupView { ViewModel = vm };
            ((Xamarin.Utilities.ViewModels.IRoutableViewModel)vm).RequestNavigation.Subscribe(x =>
            {
                var toViewType = viewModelViews.GetViewFor(x.GetType());
                var toView = serviceConstructor.Construct(toViewType) as IViewFor;
                toView.ViewModel = x;
                transitionOrchestration.Transition(startupViewController, toView);
            });

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
            var features = Locator.Current.GetService<IFeaturesService>();

            // Automatic activations in debug mode!
#if DEBUG
            Locator.Current.GetService<IDefaultValueService>().Set(FeatureIds.PushNotifications, true);
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

            var app = Locator.Current.GetService<IApplicationService>();
            var accounts = Locator.Current.GetService<IAccountsService>();
            if (app.Account != null && !app.Account.IsPushNotificationsEnabled.HasValue)
            {
                Task.Run(() => Locator.Current.GetService<IPushNotificationsService>().Register());
                app.Account.IsPushNotificationsEnabled = true;
                accounts.Update(app.Account);
            }
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
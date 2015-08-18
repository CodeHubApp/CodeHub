using System;
using Foundation;
using UIKit;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using ReactiveUI;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.App;
using Splat;
using CodeHub.iOS.Services;
using CodeHub.Core.Utilities;
using CodeHub.iOS.Factories;
using CodeHub.Core.Factories;
using System.Linq;
using CodeHub.Core;
using System.Net.Http;
using ModernHttpClient;
using CodeHub.Core.Data;
using CodeHub.iOS.ViewControllers.Settings;
using CodeHub.iOS.ViewControllers.Walkthrough;
using CodeHub.iOS.ViewControllers.App;

namespace CodeHub.iOS
{
    /// <summary>
    /// The UIApplicationDelegate for the application. This class is responsible for launching the 
    /// User Interface of the application, as well as listening (and optionally responding) to 
    /// application events from iOS.
    /// </summary>
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate, IEnableLogger
    {
        private NSObject _settingsChangedObserver;
        public string DeviceToken;

        /// <summary>
        /// The window.
        /// </summary>
        public override UIWindow Window { get; set; }

        /// <summary>
        /// Whether the application is Pro
        /// </summary>
        public bool IsPro { get; set; }

        /// <summary>
        /// This is the main entry point of the application.
        /// </summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {
            try
            {
                UIApplication.Main(args, null, "AppDelegate");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
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
            #if DEBUG
            Locator.CurrentMutable.Register(() => new DiagnosticLogger(), typeof(ILogger));
            #endif 

            // Load the launch screen. We'll need to fade.
            var storyboard = UIStoryboard.FromName("Launch", NSBundle.MainBundle);
            var viewController = storyboard.InstantiateInitialViewController();
            Window = new UIWindow(UIScreen.MainScreen.Bounds) { RootViewController = viewController };
            Window.MakeKeyAndVisible();
            BeginInvokeOnMainThread(() => InitializeApp(options));
            return true;
        }

        private void InitializeApp(NSDictionary options)
        {
            // Stamp the date this was installed (first run)
            var installedDate = this.StampInstallDate("CodeHub", DateTime.Now.ToString());
            if (installedDate < new DateTime(2015, 12, 5))
                IsPro = true;

            // Register default settings from the settings.bundle
            RegisterDefaultSettings();

            Locator.CurrentMutable.InitializeFactories();
            Locator.CurrentMutable.InitializeServices();
            Bootstrap.Init();

            _settingsChangedObserver = NSNotificationCenter.DefaultCenter.AddObserver((NSString)"NSUserDefaultsDidChangeNotification", DefaultsChanged); 

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            OctokitClientFactory.CreateMessageHandler = () => new HttpMessageHandler();
            GitHubSharp.Client.ClientConstructor = () => new HttpClient(new HttpMessageHandler());

            var viewModelViews = Locator.Current.GetService<IViewModelViewService>();
            var defaultValueService = Locator.Current.GetService<IDefaultValueService>();
            viewModelViews.RegisterViewModels(typeof(SettingsViewController).Assembly);

            Theme.Setup();

            try
            {
                Data.LegacyMigration.Migrate(Locator.Current.GetService<IAccountsRepository>());
            }
            catch (Exception e)
            {
                this.Log().DebugException("Unable to migrate db!", e);
            }

            bool hasSeenWelcome;
            if (!defaultValueService.TryGet("HAS_SEEN_WELCOME_INTRO", out hasSeenWelcome) || !hasSeenWelcome)
            {
                defaultValueService.Set("HAS_SEEN_WELCOME_INTRO", true);
                var welcomeViewController = new WelcomePageViewController();
                welcomeViewController.WantsToDimiss += GoToStartupView;
                TransitionToViewController(welcomeViewController);
            }
            else
            {
                GoToStartupView();
            }

            SetupPushNotifications();
            HandleNotificationOptions(options);
        }

        private void GoToStartupView()
        {
            var serviceConstructor = Locator.Current.GetService<IServiceConstructor>();
            var vm = serviceConstructor.Construct<StartupViewModel>();
            var startupViewController = new StartupViewController {ViewModel = vm};

            var mainNavigationController = new UINavigationController(startupViewController) { NavigationBarHidden = true };
            MessageBus.Current.Listen<LogoutMessage>().Subscribe(_ => {
                mainNavigationController.PopToRootViewController(false);
                mainNavigationController.DismissViewController(true, null);
            });

            TransitionToViewController(mainNavigationController);
        }

        private void TransitionToViewController(UIViewController viewController)
        {
            UIView.Transition(Window, 0.35, UIViewAnimationOptions.TransitionCrossDissolve, () => 
                Window.RootViewController = viewController, null);
        }

        private static void RegisterDefaultSettings()
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            var appDefaults = new NSMutableDictionary(); 
            appDefaults.SetValueForKey(NSObject.FromObject(true), new NSString("CollectAnonymousUsage"));
            userDefaults.RegisterDefaults(appDefaults);
            userDefaults.Synchronize();
        }

        private static void DefaultsChanged( NSNotification obj )
        {   
            if (ObjCRuntime.Runtime.Arch != ObjCRuntime.Arch.SIMULATOR)
            {
                var analyticsEnabled = NSUserDefaults.StandardUserDefaults.BoolForKey("CollectAnonymousUsage");
                Locator.Current.GetService<IAnalyticsService>().Enabled = analyticsEnabled;
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

        private void HandleNotificationOptions(NSDictionary options)
        {
            if (options == null) return;
            if (!options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey)) return;

            var remoteNotification = options[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
            if (remoteNotification != null)
                HandleNotification(remoteNotification, true).ToBackground();
        }

        private void SetupPushNotifications()
        {
            var features = Locator.Current.GetService<IFeaturesService>();

            // Automatic activations in debug mode!
#if DEBUG
            //Locator.Current.GetService<IDefaultValueService>().Set(FeatureIds.PushNotifications, true);
#endif

            // Notifications don't work on teh simulator so don't bother
            if (ObjCRuntime.Runtime.Arch != ObjCRuntime.Arch.SIMULATOR && features.IsPushNotificationsActivated)
                RegisterForNotifications();
        }

        private static void RegisterForNotifications()
        {
            var notificationTypes = UIUserNotificationSettings.GetSettingsForTypes (UIUserNotificationType.Alert | UIUserNotificationType.Sound, null);
            UIApplication.SharedApplication.RegisterUserNotificationSettings(notificationTypes);
        }

        public override void DidRegisterUserNotificationSettings (UIApplication application, UIUserNotificationSettings notificationSettings)
        {
            application.RegisterForRemoteNotifications();
        }


        // TODO: IMPORTANT!!!
//        void HandlePurchaseSuccess (object sender, string e)
//        {
//            IoC.Resolve<IDefaultValueService>().Set(e, true);
//
//            if (string.Equals(e, FeatureIds.PushNotifications))
//            {
//                RegisterForNotifications();
//            }
//        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            if (application.ApplicationState == UIApplicationState.Active)
                return;
            HandleNotification(userInfo, false).ToBackground();
        }

        private async Task HandleNotification(NSDictionary data, bool fromBootup)
        {
            var dic = data.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
            var pushNotificationService = Locator.Current.GetService<IPushNotificationService>();
            var accountsRepository = Locator.Current.GetService<IAccountsRepository>();
            var sessionService = Locator.Current.GetService<ISessionService>();
            var action = pushNotificationService.Handle(new PushNotificationRequest(dic));
            if (action == null)
                return;

            var account = (await accountsRepository.GetAll()).FirstOrDefault(x => string.Equals(x.Username, action.Username, StringComparison.OrdinalIgnoreCase));
            var changeAccount = account != null && account != sessionService.Account;

            sessionService.StartupViewModel = action.ViewModel;

            if (!fromBootup && changeAccount)
            {
                await Locator.Current.GetService<IAccountsRepository>().SetDefault(account);
                MessageBus.Current.SendMessage(new LogoutMessage());
            }
            if (!fromBootup && !changeAccount)
            {
                var nav = Window?.RootViewController as UINavigationController;
                var menuView = nav?.ViewControllers.OfType<MenuViewController>().FirstOrDefault();
                menuView?.ViewModel?.ActivateCommand.ExecuteIfCan();
            }
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
            return true;
        }
    }
}
using System.Collections.Generic;
using System;
using Foundation;
using UIKit;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using System.Linq;
using ObjCRuntime;
using System.Net.Http;
using CodeHub.iOS.Services;
using ReactiveUI;
using CodeHub.Core.Messages;
using CodeHub.iOS.XCallback;
using System.Reactive.Linq;
using Splat;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace CodeHub.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public string DeviceToken;

        public override UIWindow Window { get; set; }

        public static AppDelegate Instance => UIApplication.SharedApplication.Delegate as AppDelegate;

        /// <summary>
        /// This is the main entry point of the application.
        /// </summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            AppCenter.Start("eef367be-437c-4c67-abe0-79779b3b8392", typeof(Analytics), typeof(Crashes));

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            var culture = new System.Globalization.CultureInfo("en");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Setup theme
            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, true);
            Theme.Setup();

            var accountsService = new AccountsService();
            var applicationService = new ApplicationService(accountsService);
            var inAppPurchaseService = new InAppPurchaseService();
            var featuresService = new FeaturesService(inAppPurchaseService);

            Locator.CurrentMutable.RegisterConstant<IApplicationService>(applicationService);
            Locator.CurrentMutable.RegisterConstant<IAccountsService>(accountsService);
            Locator.CurrentMutable.RegisterConstant<IAlertDialogService>(new AlertDialogService());
            Locator.CurrentMutable.RegisterConstant<INetworkActivityService>(new NetworkActivityService());
            Locator.CurrentMutable.RegisterConstant<IMessageService>(new MessageService());
            Locator.CurrentMutable.RegisterConstant<IInAppPurchaseService>(inAppPurchaseService);
            Locator.CurrentMutable.RegisterConstant<IFeaturesService>(featuresService);
            Locator.CurrentMutable.RegisterConstant<ILoginService>(new LoginService(accountsService, applicationService));
            Locator.CurrentMutable.RegisterConstant<IMarkdownService>(new MarkdownService());
            Locator.CurrentMutable.RegisterConstant<IPushNotificationsService>(new PushNotificationsService());

            Locator.CurrentMutable.RegisterLazySingleton(
                () => new ImgurService(), typeof(IImgurService));

            inAppPurchaseService.ThrownExceptions.Subscribe(ex =>
            {
                var error = new Core.UserError("Error Purchasing", ex.Message);
                Core.Interactions.Errors.Handle(error).Subscribe();
            });

            Core.Interactions.Errors.RegisterHandler(interaction =>
            {
                var error = interaction.Input;
                AlertDialogService.ShowAlert(error.Title, error.Message);
                interaction.SetOutput(System.Reactive.Unit.Default);
            });

#if DEBUG
            featuresService.ActivateProDirect();
#endif 

            //options = new NSDictionary (UIApplication.LaunchOptionsRemoteNotificationKey, 
                //new NSDictionary ("r", "octokit/octokit.net", "i", "739", "u", "thedillonb"));

            if (options != null)
            {
                if (options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey)) 
                {
                    var remoteNotification = options[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
                    if(remoteNotification != null) {
                        HandleNotification(remoteNotification, true);
                    }
                }
            }

            if (!Core.Settings.HasSeenWelcome)
            {
                Core.Settings.HasSeenWelcome = true;
                var welcomeViewController = new ViewControllers.Walkthrough.WelcomePageViewController();
                welcomeViewController.WantsToDimiss += GoToStartupView;
                TransitionToViewController(welcomeViewController);
            }
            else
            {
                GoToStartupView();
            }

            Window.MakeKeyAndVisible();

            // Notifications don't work on teh simulator so don't bother
            if (Runtime.Arch != Arch.SIMULATOR && featuresService.IsProEnabled)
                RegisterUserForNotifications();

            return true;
        }

        public void RegisterUserForNotifications()
        {
            var notificationTypes = UIUserNotificationSettings.GetSettingsForTypes (UIUserNotificationType.Alert | UIUserNotificationType.Sound, null);
            UIApplication.SharedApplication.RegisterUserNotificationSettings(notificationTypes);
        }

        private void GoToStartupView()
        {
            TransitionToViewController(new ViewControllers.Application.StartupViewController());

            MessageBus
                .Current.Listen<LogoutMessage>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => new ViewControllers.Application.StartupViewController())
                .Subscribe(TransitionToViewController);
        }

        public void TransitionToViewController(UIViewController viewController)
        {
            UIView.Transition(Window, 0.35, UIViewAnimationOptions.TransitionCrossDissolve, () => 
                Window.RootViewController = viewController, null);
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            if (application.ApplicationState == UIApplicationState.Active)
                return;
            HandleNotification(userInfo, false);
        }

        private void HandleNotification(NSDictionary data, bool fromBootup)
        {
            try
            {
                var appService = Locator.Current.GetService<IApplicationService>();
                var accountsService = Locator.Current.GetService<IAccountsService>();
                var repoId = RepositoryIdentifier.FromFullName(data["r"].ToString());
                var parameters = new Dictionary<string, string>() {{"Username", repoId?.Owner}, {"Repository", repoId?.Name}};

                if (data.ContainsKey(new NSString("c")))
                {
                    //request = MvxViewModelRequest<CodeHub.Core.ViewModels.Changesets.ChangesetViewModel>.GetDefaultRequest();
                    parameters.Add("Node", data["c"].ToString());
                    parameters.Add("ShowRepository", "True");
                }
                else if (data.ContainsKey(new NSString("i")))
                {
                    //request = MvxViewModelRequest<CodeHub.Core.ViewModels.Issues.IssueViewModel>.GetDefaultRequest();
                    parameters.Add("Id", data["i"].ToString());
                }
                else if (data.ContainsKey(new NSString("p")))
                {
                    //request = MvxViewModelRequest<CodeHub.Core.ViewModels.PullRequests.PullRequestViewModel>.GetDefaultRequest();
                    parameters.Add("Id", data["p"].ToString());
                }
                else
                {
                    //request = MvxViewModelRequest<CodeHub.Core.ViewModels.Repositories.RepositoryViewModel>.GetDefaultRequest();
                }
      
                var username = data["u"].ToString();

                if (appService.Account == null || !appService.Account.Username.Equals(username))
                {
                    var accounts = accountsService.GetAccounts().Result.ToList();

                    var user = accounts.FirstOrDefault(x => x.Username.Equals(username));
                    if (user != null)
                    {
                        appService.DeactivateUser();
                        accountsService.SetActiveAccount(user).Wait();
                    }
                }

                appService.SetUserActivationAction(() => {
                    //TODO: FIX THIS
                });

                if (appService.Account == null && !fromBootup)
                {
                    MessageBus.Current.SendMessage(new LogoutMessage());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Handle Notifications issue: " + e);
            }
        }

        public override void DidRegisterUserNotificationSettings (UIApplication application, UIUserNotificationSettings notificationSettings)
        {
            application.RegisterForRemoteNotifications ();
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            DeviceToken = deviceToken.Description.Trim('<', '>').Replace(" ", "");

            var app = Locator.Current.GetService<IApplicationService>();
            var accounts = Locator.Current.GetService<IAccountsService>();
            if (app.Account != null && !app.Account.IsPushNotificationsEnabled.HasValue)
            {
                Locator.Current.GetService<IPushNotificationsService>().Register().ToBackground();
                app.Account.IsPushNotificationsEnabled = true;
                accounts.Save(app.Account);
            }
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            AlertDialogService.ShowAlert("Error Registering for Notifications", error.LocalizedDescription);
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            var uri = new Uri(url.AbsoluteString);

            if (uri.Host == "x-callback-url")
            {
                XCallbackProvider.Handle(new XCallbackQuery(url.AbsoluteString));
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
//                var first = path.Substring(0, path.IndexOf("/", StringComparison.Ordinal));
                return UrlRouteProvider.Handle(path);
            }
        }
    }
}
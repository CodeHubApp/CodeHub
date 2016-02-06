// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the AppDelegate type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CodeFramework.iOS;
using System.Collections.Generic;
using System;    
using MvvmCross.Core.ViewModels;
using Foundation;
using UIKit;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using CodeFramework.iOS.XCallback;
using Security;
using ObjCRuntime;
using System.Net;
using MvvmCross.iOS.Platform;
using MvvmCross.Platform;
using MvvmCross.Core.Views;

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
		public string DeviceToken;

		public override UIWindow Window {
			get;
			set;
		}

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
			this.Window = new UIWindow(UIScreen.MainScreen.Bounds);
			var presenter = new IosViewPresenter(this.Window);
            var setup = new Setup(this, presenter);
            setup.Initialize();

            // Setup theme
            Theme.Setup();
//
//			options = new NSDictionary (UIApplication.LaunchOptionsRemoteNotificationKey, 
//				new NSDictionary ("r", "octokit/octokit.net", "i", "739", "u", "thedillonb"));
//
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

            var startup = Mvx.Resolve<IMvxAppStart>();
			startup.Start();

			this.Window.MakeKeyAndVisible();

            this.StampInstallDate("CodeHub");

            InAppPurchases.Instance.PurchaseError += HandlePurchaseError;
            InAppPurchases.Instance.PurchaseSuccess += HandlePurchaseSuccess;

            var features = Mvx.Resolve<IFeaturesService>();

            // Automatic activations in debug mode!
            #if DEBUG
            Mvx.Resolve<CodeHub.Core.Services.IDefaultValueService>().Set(FeatureIds.PushNotifications, true);
            #endif


			// Notifications don't work on teh simulator so don't bother
            if (ObjCRuntime.Runtime.Arch != ObjCRuntime.Arch.SIMULATOR && features.IsPushNotificationsActivated)
			{
				var notificationTypes = UIUserNotificationSettings.GetSettingsForTypes (UIUserNotificationType.Alert | UIUserNotificationType.Sound, null);
				UIApplication.SharedApplication.RegisterUserNotificationSettings(notificationTypes);
			}

            return true;
        }

        public override void WillEnterForeground(UIApplication application)
        {
            XCallbackProvider.DestoryTokens();
            base.WillEnterForeground(application);
        }

        void HandlePurchaseSuccess (object sender, string e)
        {
            Mvx.Resolve<CodeHub.Core.Services.IDefaultValueService>().Set(e, true);

            if (string.Equals(e, FeatureIds.PushNotifications))
            {
				var notificationTypes = UIUserNotificationSettings.GetSettingsForTypes (UIUserNotificationType.Alert | UIUserNotificationType.Sound, null);
				UIApplication.SharedApplication.RegisterUserNotificationSettings(notificationTypes);
            }
        }

        void HandlePurchaseError (object sender, Exception e)
        {
            MonoTouch.Utilities.ShowAlert("Unable to make purchase", e.Message);
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
				var viewDispatcher = Mvx.Resolve<IMvxViewDispatcher>();
                var appService = Mvx.Resolve<IApplicationService>();
                var repoId = new RepositoryIdentifier(data["r"].ToString());
                var parameters = new Dictionary<string, string>() {{"Username", repoId.Owner}, {"Repository", repoId.Name}};

                MvxViewModelRequest request;
                if (data.ContainsKey(new NSString("c")))
                {
                    request = MvxViewModelRequest<CodeHub.Core.ViewModels.Changesets.ChangesetViewModel>.GetDefaultRequest();
                    parameters.Add("Node", data["c"].ToString());
                    parameters.Add("ShowRepository", "True");
                }
                else if (data.ContainsKey(new NSString("i")))
                {
                    request = MvxViewModelRequest<CodeHub.Core.ViewModels.Issues.IssueViewModel>.GetDefaultRequest();
                    parameters.Add("Id", data["i"].ToString());
                }
                else if (data.ContainsKey(new NSString("p")))
                {
                    request = MvxViewModelRequest<CodeHub.Core.ViewModels.PullRequests.PullRequestViewModel>.GetDefaultRequest();
                    parameters.Add("Id", data["p"].ToString());
                }
                else
                {
                    request = MvxViewModelRequest<CodeHub.Core.ViewModels.Repositories.RepositoryViewModel>.GetDefaultRequest();
                }

                request.ParameterValues = parameters;

                var username = data["u"].ToString();

                if (appService.Account == null || !appService.Account.Username.Equals(username))
                {
                    var user = appService.Accounts.FirstOrDefault(x => x.Username.Equals(username));
                    if (user != null)
                    {
                        appService.DeactivateUser();
                        appService.Accounts.SetDefault(user);
                    }
                }

                appService.SetUserActivationAction(() => viewDispatcher.ShowViewModel(request));

                if (appService.Account == null && !fromBootup)
                {
                    var startupViewModelRequest = MvxViewModelRequest<CodeHub.Core.ViewModels.App.StartupViewModel>.GetDefaultRequest();
                    viewDispatcher.ShowViewModel(startupViewModelRequest);
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

            var app = Mvx.Resolve<IApplicationService>();
            if (app.Account != null && !app.Account.IsPushNotificationsEnabled.HasValue)
            {
                Task.Run(() => Mvx.Resolve<IPushNotificationsService>().Register());
                app.Account.IsPushNotificationsEnabled = true;
                app.Accounts.Update(app.Account);
            }
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			MonoTouch.Utilities.ShowAlert("Error Registering for Notifications", error.LocalizedDescription);
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
                var first = path.Substring(0, path.IndexOf("/", StringComparison.Ordinal));
                var firstIsDomain = first.Contains(".");

                return UrlRouteProvider.Handle(path);
            }
        }
    }

	public static class UIApplicationDelegateExtensions
	{
		/// <summary>
		/// Record the date this application was installed (or the date that we started recording installation date).
		/// </summary>
		public static DateTime StampInstallDate(this UIApplicationDelegate @this, string name)
		{
			try
			{
				var query = new SecRecord(SecKind.GenericPassword) { Service = name, Account = "account" };

				SecStatusCode secStatusCode;
				var queriedRecord = SecKeyChain.QueryAsRecord(query, out secStatusCode);
				if (secStatusCode != SecStatusCode.Success)
				{
					queriedRecord = new SecRecord(SecKind.GenericPassword)
					{
						Label = name + " Install Date",
						Service = name,
                        Account = query.Account,
						Description = string.Format("The first date {0} was installed", name),
						Generic = NSData.FromString(DateTime.UtcNow.ToString())
					};

					var err = SecKeyChain.Add(queriedRecord);
					if (err != SecStatusCode.Success)
						System.Diagnostics.Debug.WriteLine("Unable to save stamp date!");
				}
				else
				{
					DateTime time;
					if (!DateTime.TryParse(queriedRecord.Generic.ToString(), out time))
						SecKeyChain.Remove(query);
				}

				return DateTime.Parse(NSString.FromData(queriedRecord.Generic, NSStringEncoding.UTF8));
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				return DateTime.Now;
			}
		}
	}
}
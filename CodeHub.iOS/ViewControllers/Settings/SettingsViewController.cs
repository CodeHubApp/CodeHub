using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core;
using CodeHub.Core.Services;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers.Application;
using CodeHub.iOS.ViewControllers.Repositories;
using Foundation;
using Humanizer;
using ReactiveUI;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Settings
{
    public class SettingsViewController : BaseDialogViewController
    {
        private static string RegisterNotificationsError = "Unable to register for push notifications!";
        private readonly IFeaturesService _featuresService = Locator.Current.GetService<IFeaturesService>();
        private readonly IApplicationService _applicationService = Locator.Current.GetService<IApplicationService>();
        private readonly IAlertDialogService _alertDialogService = Locator.Current.GetService<IAlertDialogService>();
        private readonly IPushNotificationsService _pushNotificationsService = Locator.Current.GetService<IPushNotificationsService>();
        private readonly ReactiveCommand<bool, Unit> _registerPushNotifications;

        public SettingsViewController()
        {
            Title = "Settings";

            _registerPushNotifications = ReactiveCommand.CreateFromTask<bool>(RegisterPushNotifications);

            _registerPushNotifications
                .ThrownExceptions
                .Select(error => new UserError(RegisterNotificationsError, error))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            _registerPushNotifications.Subscribe(_ => CreateTable());

            Appearing.Subscribe(_ => CreateTable());
        }

        private async Task RegisterPushNotifications(bool enabled)
        {
            if (!_featuresService.IsProEnabled)
            {
                var response = await _alertDialogService.PromptYesNo(
                    "Requires Activation",
                    "Push notifications require activation. Would you like to go there now to activate push notifications?");

                if (response)
                    UpgradeViewController.Present(this);

                return;
            }

            if (enabled)
                await _pushNotificationsService.Register();
            else
                await _pushNotificationsService.Deregister();

            _applicationService.Account.IsPushNotificationsEnabled = enabled;
            await _applicationService.UpdateActiveAccount();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.Image = Images.LoginUserUnknown;
            HeaderView.SubText = "Settings apply to this account only.";

            CreateTable();
        }

        private void CreateTable()
        {
            var currentAccount = _applicationService.Account;
            var accountSection = new Section("Account");

            var showOrganizationsInEvents = new BooleanElement("Show Organizations in Events", currentAccount.ShowOrganizationsInEvents);
            showOrganizationsInEvents.Changed.Subscribe(x => {
                currentAccount.ShowOrganizationsInEvents = x;
                _applicationService.UpdateActiveAccount().ToBackground();
            });

            var showOrganizations = new BooleanElement("List Organizations in Menu", currentAccount.ExpandOrganizations);
            showOrganizations.Changed.Subscribe(x => { 
                currentAccount.ExpandOrganizations = x;
                _applicationService.UpdateActiveAccount().ToBackground();
            });

            var repoDescriptions = new BooleanElement("Show Repo Descriptions", currentAccount.ShowRepositoryDescriptionInList);
            repoDescriptions.Changed.Subscribe(x => {
                currentAccount.ShowRepositoryDescriptionInList = x;
                _applicationService.UpdateActiveAccount().ToBackground();
            });

            var startupView = new StringElement(
                "Startup View", _applicationService.Account.DefaultStartupView, UITableViewCellStyle.Value1)
            { 
                Accessory = UITableViewCellAccessory.DisclosureIndicator,
            };

            startupView.Clicked.Subscribe(_ =>
            {
                var viewController = new DefaultStartupViewController(
                    () => NavigationController.PopToViewController(this, true));
                NavigationController.PushViewController(viewController, true);
            });

            var syntaxHighlighter = new ButtonElement(
                "Syntax Highlighter",
                _applicationService.Account.CodeEditTheme?.Humanize(LetterCasing.Title) ?? "Default");

            syntaxHighlighter
                .Clicked
                .Select(_ => new SyntaxHighlighterViewController())
                .Subscribe(vc => this.PushViewController(vc));

            var pushNotifications = new BooleanElement(
                "Push Notifications",
                _applicationService.Account.IsPushNotificationsEnabled == true);

            pushNotifications
                .Changed
                .InvokeReactiveCommand(_registerPushNotifications);

            accountSection.Add(pushNotifications);
       
            var source = new StringElement("Source Code");
            source
                .Clicked
                .Select(_ => RepositoryViewController.CreateCodeHubViewController())
                .Subscribe(vc => this.PushViewController(vc));
                  
            var follow = new StringElement("Follow On Twitter");
            follow
                .Clicked
                .Select(_ => new NSUrl("https://twitter.com/CodeHubapp"))
                .Subscribe(url => UIApplication.SharedApplication.OpenUrl(url));

            var rate = new StringElement("Rate This App");
            rate
                .Clicked
                .Select(_ => new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8"))
                .Subscribe(url => UIApplication.SharedApplication.OpenUrl(url));

            var aboutSection = new Section("About", "Thank you for downloading. Enjoy!")
            {
                source,
                follow,
                rate
            };
        
            if (_featuresService.IsProEnabled)
            {
                var upgrades = new StringElement("Upgrades");
                upgrades.Clicked.Subscribe(_ => UpgradeViewController.Present(this));
                aboutSection.Add(upgrades);
            }

            var appVersion = new StringElement("App Version", UIApplication.SharedApplication.GetVersion())
            { 
                Accessory = UITableViewCellAccessory.None,
                SelectionStyle = UITableViewCellSelectionStyle.None
            };

            aboutSection.Add(appVersion);

            var appearanceSection = new Section("Appearance")
            {
                showOrganizationsInEvents,
                showOrganizations,
                repoDescriptions,
                startupView,
                syntaxHighlighter
            };

            Root.Reset(accountSection, appearanceSection, aboutSection);
        }
    }
}



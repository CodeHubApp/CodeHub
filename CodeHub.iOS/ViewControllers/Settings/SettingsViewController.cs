using System;
using UIKit;
using Foundation;
using ReactiveUI;
using CodeHub.Core.ViewModels.Settings;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Settings
{
    public class SettingsViewController : BaseDialogViewController<SettingsViewModel>
	{
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.Image = Images.LoginUserUnknown;
            HeaderView.SubText = "Settings apply to this account only.";

            var showOrganizationsInEvents = new BooleanElement("Show Organizations in Events", ViewModel.ShowOrganizationsInEvents);
            var showOrganizations = new BooleanElement("List Organizations in Menu", ViewModel.ExpandOrganizations);
            var repoDescriptions = new BooleanElement("Show Repo Descriptions", ViewModel.ShowRepositoryDescriptionInList);
            var startupView = new StringElement("Startup View", ViewModel.DefaultStartupViewName, UITableViewCellStyle.Value1);
            var syntaxHighlighter = new StringElement("Syntax Highlighter", ViewModel.SyntaxHighlighter, UITableViewCellStyle.Value1);
            var applicationSection = new Section("Application", "Looking for application settings? They're located in the iOS settings application.");

            var version = UIDevice.CurrentDevice.SystemVersion.Split('.');
            var major = Int32.Parse(version[0]);
            var settingsElement = new StringElement("Go To Application Settings");

            if (major >= 8)
            {
                applicationSection.Add(settingsElement);
            }

            var appearanceSection = new Section("Appearance") 
            {
                showOrganizationsInEvents,
                showOrganizations,
                repoDescriptions,
                startupView,
                syntaxHighlighter
            };

            var followElement = new StringElement("Follow On Twitter");
            var rateElement = new StringElement("Rate This App");
            var sourceElement = new StringElement("Source Code");

            var aboutSection = new Section("About", "Thank you for downloading. Enjoy!")
            {
                followElement,
                rateElement,
                sourceElement,
                new StringElement("App Version", ViewModel.Version)
            };

            Root.Reset(appearanceSection, applicationSection, aboutSection);

            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.AccountImageUrl).SubscribeSafe(x => HeaderView.SetImage(new Uri(x), Images.LoginUserUnknown)));
                d(this.WhenAnyValue(x => x.ViewModel.ShowOrganizationsInEvents).Subscribe(x => showOrganizationsInEvents.Value = x));

                d(this.WhenAnyValue(x => x.ViewModel.DefaultStartupViewName).Subscribe(x => {
                    startupView.Value = x;
                    Root.Reload(startupView);
                }));

                d(this.WhenAnyValue(x => x.ViewModel.SyntaxHighlighter).Subscribe(x => {
                    syntaxHighlighter.Value = x;
                    Root.Reload(syntaxHighlighter);
                }));

                d(showOrganizationsInEvents.Changed.Subscribe(x => ViewModel.ShowOrganizationsInEvents = x));

                d(this.WhenAnyValue(x => x.ViewModel.ExpandOrganizations).Subscribe(x => showOrganizations.Value = x));
                d(showOrganizations.Changed.Subscribe(x => ViewModel.ExpandOrganizations = x));
                d(this.WhenAnyValue(x => x.ViewModel.ShowRepositoryDescriptionInList).Subscribe(x => repoDescriptions.Value = x));
                d(repoDescriptions.Changed.Subscribe(x => ViewModel.ShowRepositoryDescriptionInList = x));

                d(followElement.Clicked.Subscribe(_ => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp"))));
                d(rateElement.Clicked.Subscribe(_ => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8"))));
                d(sourceElement.Clicked.InvokeCommand(ViewModel.GoToSourceCodeCommand));

                d(startupView.Clicked.InvokeCommand(ViewModel.GoToDefaultStartupViewCommand));
                d(syntaxHighlighter.Clicked.InvokeCommand(ViewModel.GoToSyntaxHighlighterCommand));
                d(settingsElement.Clicked.Subscribe(_ => UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString))));
            });
        }
    }
}



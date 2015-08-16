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

            this.WhenAnyValue(x => x.ViewModel.AccountImageUrl)
                .SubscribeSafe(x => HeaderView.SetImage(new Uri(x), Images.LoginUserUnknown));

            var showOrganizationsInEvents = new BooleanElement("Show Organizations in Events", 
                ViewModel.ShowOrganizationsInEvents, e => ViewModel.ShowOrganizationsInEvents = e.Value);
            ViewModel.WhenAnyValue(x => x.ShowOrganizationsInEvents).Subscribe(x => showOrganizationsInEvents.Value = x);

            var showOrganizations = new BooleanElement("List Organizations in Menu", 
                ViewModel.ExpandOrganizations, e => ViewModel.ExpandOrganizations = e.Value);
            ViewModel.WhenAnyValue(x => x.ExpandOrganizations).Subscribe(x => showOrganizations.Value = x);

            var repoDescriptions = new BooleanElement("Show Repo Descriptions",
                ViewModel.ShowRepositoryDescriptionInList, e => ViewModel.ShowRepositoryDescriptionInList = e.Value);
            ViewModel.WhenAnyValue(x => x.ShowRepositoryDescriptionInList).Subscribe(x => repoDescriptions.Value = x);

            var startupView = new StringElement("Startup View", ViewModel.DefaultStartupViewName, UITableViewCellStyle.Value1);
            startupView.Tapped = () => ViewModel.GoToDefaultStartupViewCommand.ExecuteIfCan();
            ViewModel.WhenAnyValue(x => x.DefaultStartupViewName).Subscribe(x => 
            {
                startupView.Value = x;
                Root.Reload(startupView);
            });

            var syntaxHighlighter = new StringElement("Syntax Highlighter", ViewModel.SyntaxHighlighter, UITableViewCellStyle.Value1);
            syntaxHighlighter.Tapped = () => ViewModel.GoToSyntaxHighlighterCommand.ExecuteIfCan();
            ViewModel.WhenAnyValue(x => x.SyntaxHighlighter).Subscribe(x => 
            {
                syntaxHighlighter.Value = x;
                Root.Reload(syntaxHighlighter);
            });

            var applicationSection = new Section("Application", "Looking for application settings? They're located in the iOS settings application.");

            var version = UIDevice.CurrentDevice.SystemVersion.Split('.');
            var major = Int32.Parse(version[0]);

            if (major >= 8)
            {
                applicationSection.Add(
                    new StringElement("Go To Application Settings", 
                        () => UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString))));
            }

            var appearanceSection = new Section("Appearance") 
            {
                showOrganizationsInEvents,
                showOrganizations,
                repoDescriptions,
                startupView,
                syntaxHighlighter
            };

            var aboutSection = new Section("About", "Thank you for downloading. Enjoy!")
            {
                new StringElement("Follow On Twitter", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp"))),
                new StringElement("Rate This App", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8"))),
                new StringElement("Source Code", () => ViewModel.GoToSourceCodeCommand.ExecuteIfCan()),
                new StringElement("App Version", ViewModel.Version)
            };

            Root.Reset(appearanceSection, applicationSection, aboutSection);
        }
    }
}



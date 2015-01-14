using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Settings;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Settings
{
    public class SettingsView : BaseDialogViewController<SettingsViewModel>
	{
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.SubText = "Settings apply to this account only.";
            HeaderView.Image = Images.LoginUserUnknown;
            ViewModel.WhenAnyValue(x => x.AccountImageUrl).IsNotNull().Subscribe(x => HeaderView.ImageUri = x);

            var saveCredentials = new BooleanElement("Save Credentials", 
                ViewModel.SaveCredentials, e => ViewModel.SaveCredentials = e.Value);
            ViewModel.WhenAnyValue(x => x.SaveCredentials).Subscribe(x => saveCredentials.Value = x);

            var showOrganizationsInEvents = new BooleanElement("Show Organizations in Events", 
                ViewModel.ShowOrganizationsInEvents, e => ViewModel.ShowOrganizationsInEvents = e.Value);
            ViewModel.WhenAnyValue(x => x.ShowOrganizationsInEvents).Subscribe(x => showOrganizationsInEvents.Value = x);

            var showOrganizations = new BooleanElement("List Organizations in Menu", 
                ViewModel.ExpandOrganizations, e => ViewModel.ExpandOrganizations = e.Value);
            ViewModel.WhenAnyValue(x => x.ExpandOrganizations).Subscribe(x => showOrganizations.Value = x);

            var repoDescriptions = new BooleanElement("Show Repo Descriptions",
                ViewModel.ShowRepositoryDescriptionInList, e => ViewModel.ShowRepositoryDescriptionInList = e.Value);
            ViewModel.WhenAnyValue(x => x.ShowRepositoryDescriptionInList).Subscribe(x => repoDescriptions.Value = x);

            var startupView = new StyledStringElement("Startup View", ViewModel.DefaultStartupViewName, UITableViewCellStyle.Value1) { 
              Accessory = UITableViewCellAccessory.DisclosureIndicator,
            };
            startupView.Tapped += () => ViewModel.GoToDefaultStartupViewCommand.ExecuteIfCan();
            ViewModel.WhenAnyValue(x => x.DefaultStartupViewName).Subscribe(x => 
            {
                startupView.Value = x;
                Root.Reload(startupView);
            });

            var syntaxHighlighter = new StyledStringElement("Syntax Highlighter", ViewModel.SyntaxHighlighter, UITableViewCellStyle.Value1) { 
                Accessory = UITableViewCellAccessory.DisclosureIndicator,
            };
            syntaxHighlighter.Tapped += () => ViewModel.GoToSyntaxHighlighterCommand.ExecuteIfCan();
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
                    new StyledStringElement("Go To Application Settings", 
                        () => UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString)))
                        { Accessory = UITableViewCellAccessory.DisclosureIndicator });
            }

            var accountSection = new Section("Account") { saveCredentials };

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
                new StyledStringElement("Follow On Twitter", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp"))),
                new StyledStringElement("Rate This App", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8"))),
                new StyledStringElement("Source Code", () => ViewModel.GoToSourceCodeCommand.ExecuteIfCan()),
                new StyledStringElement("App Version", ViewModel.Version)
            };

            Root.Reset(accountSection, appearanceSection, applicationSection, aboutSection);
        }
    }
}



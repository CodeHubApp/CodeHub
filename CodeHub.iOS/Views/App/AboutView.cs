using System;
using CodeFramework.iOS.Views;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.App;

namespace CodeHub.iOS.Views.App
{
	public class AboutView : ViewModelDialogView<AboutViewModel>
	{
        private const string About = @"CodeHub is the best way to browse and maintain your GitHub repositories on any iPhone, iPod Touch, and iPad device! " +
            "Keep an eye on your projects with the ability to view everything from pull requests to commenting on individual file diffs in the latest change set. " + 
            "CodeHub brings GitHub to your finger tips in a sleek and efficient design." + "\n\nCreated By Dillon Buchanan";

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var root = new RootElement("About")
            {
                new Section
                {
                    new MultilinedElement("CodeHub") { Value = About, CaptionColor = Theme.CurrentTheme.MainTitleColor, ValueColor = Theme.CurrentTheme.MainTextColor }
                },
                new Section
                {
                    new StyledStringElement("Source Code", () => ViewModel.GoToSourceCodeCommand.Execute(null))
                },
                new Section(String.Empty, "Thank you for downloading. Enjoy!")
                {
                    new StyledStringElement("Follow On Twitter", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp"))),
                    new StyledStringElement("Rate This App", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8"))),
                    new StyledStringElement("App Version", ViewModel.Version)
                }
            };

            root.UnevenRows = true;
            Root = root;
        }
    }
}


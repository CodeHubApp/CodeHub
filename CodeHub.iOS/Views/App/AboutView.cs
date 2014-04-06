using System;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeFramework.iOS.ViewControllers;
using CodeHub.Core.ViewModels.App;

namespace CodeHub.iOS.Views.App
{
	public class AboutView : ViewModelDrivenDialogViewController
    {
        private const string About = @"CodeHub is the best way to browse and maintain your GitHub repositories on any iPhone, iPod Touch, and iPad device! " +
            "Keep an eye on your projects with the ability to view everything from pull requests to commenting on individual file diffs in the latest change set. " + 
            "CodeHub brings GitHub to your finger tips in a sleek and efficient design." + "\n\nCreated By Dillon Buchanan";

        public AboutView()
            : base (true)
        {
			Title = "About".t();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (AboutViewModel)ViewModel;

            var root = new RootElement(Title)
            {
                new Section
                {
                    new MultilinedElement("CodeHub".t()) { Value = About, CaptionColor = Theme.CurrentTheme.MainTitleColor, ValueColor = Theme.CurrentTheme.MainTextColor }
                },
                new Section
                {
                    new StyledStringElement("Source Code".t(), () => vm.GoToSourceCodeCommand.Execute(null))
                },
                new Section(String.Empty, "Thank you for downloading. Enjoy!")
                {
                    new StyledStringElement("Follow On Twitter".t(), () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp"))),
                    new StyledStringElement("Rate This App".t(), () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8"))),
                    new StyledStringElement("App Version".t(), NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")).ToString())
                }
            };

            root.UnevenRows = true;
            Root = root;
        }
    }
}


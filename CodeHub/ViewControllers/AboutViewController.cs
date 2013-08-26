using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using CodeFramework.Controllers;
using CodeFramework.Elements;

namespace CodeHub.ViewControllers
{
    public class AboutViewController : BaseDialogViewController
    {
        static readonly string About = @"CodeHub is the best way to browse and maintain your Bitbucket repositories on any iOS device! " +
                "Keep an eye on your projects with the ability to view everything from followers to the individual file diffs in the latest change set. " +
                "CodeHub brings Bitbucket to your finger tips in a sleek and efficient design. " + 
                "\n\nCreated By Dillon Buchanan";


        public AboutViewController()
            : base (true)
        {
            Title = "About".t();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var root = new RootElement(Title)
            {
                new Section()
                {
                    new MultilinedElement("CodeHub".t()) { Value = About }
                },
                new Section()
                {
                    new StyledStringElement("Source Code".t(), () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://github.com/thedillonb/CodeHub")))
                },
                new Section(String.Empty, "Thank you for downloading. Enjoy!")
                {
                    new StyledStringElement("Follow On Twitter".t(), () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp"))),
                    new StyledStringElement("Rate This App".t(), () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/CodeHub/id551531422?mt=8"))),
                    new StyledStringElement("App Version".t(), NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")).ToString())
                }
            };

            root.UnevenRows = true;
            Root = root;
        }
    }
}


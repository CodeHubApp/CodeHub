using UIKit;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.ViewControllers;
using System;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class FeedbackViewController : BaseViewController
    {
        public FeedbackViewController()
            : base("FeedbackViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            GitHubButton.BackgroundColor = UIColor.FromRGB(0x7f, 0x8c, 0x8d);
            GitHubButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            GitHubButton.Layer.CornerRadius = 6f;

            UserVoiceButton.BackgroundColor = UIColor.FromRGB(0x2c, 0x3e, 0x50);
            UserVoiceButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            UserVoiceButton.Layer.CornerRadius = 6f;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            GitHubButton.TouchUpInside += ShowProject;
            UserVoiceButton.TouchUpInside += ShowUserVoice;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            GitHubButton.TouchUpInside -= ShowProject;
            UserVoiceButton.TouchUpInside -= ShowUserVoice;
        }

        private void ShowProject(object sender, EventArgs args)
        {
            ShowWebPage("https://github.com/thedillonb/codehub");
        }

        private void ShowUserVoice(object sender, EventArgs args)
        {
            ShowWebPage("https://codehub.uservoice.com");
        }

        private void ShowWebPage(string url)
        {
            var vm = new WebBrowserViewModel().Init(url);
            var view = new WebBrowserViewController(true, true) { ViewModel = vm };
            view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, 
                (s, e) => DismissViewController(true, null));
            PresentViewController(new ThemedNavigationController(view), true, null);
        }
    }
}


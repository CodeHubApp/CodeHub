using UIKit;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class FeedbackViewController : UIViewController
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
            GitHubButton.TouchUpInside += (_, __) => ShowWebPage("https://github.com/thedillonb/codehub");

            UserVoiceButton.BackgroundColor = UIColor.FromRGB(0x2c, 0x3e, 0x50);
            UserVoiceButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            UserVoiceButton.Layer.CornerRadius = 6f;
            UserVoiceButton.TouchUpInside += (_, __) => ShowWebPage("https://codehub.uservoice.com");
        }

        private void ShowWebPage(string url)
        {
            var vm = new WebBrowserViewModel().Init(url);
            var view = new WebBrowserView(true, true) { ViewModel = vm };
            view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, 
                (s, e) => DismissViewController(true, null));
            PresentViewController(new ThemedNavigationController(view), true, null);
        }
    }
}


using UIKit;
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

            OnActivation(d =>
            {
                d(GitHubButton.GetClickedObservable()
                  .Subscribe(_ => ShowWebPage("https://github.com/codehubapp/codehub")));
            });
        }

        private void ShowWebPage(string url)
        {
            var view = new WebBrowserViewController(url);
            PresentViewController(view, true, null);
        }
    }
}


using UIKit;
using System;
using System.Reactive.Linq;

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
                d(GitHubButton
                  .GetClickedObservable()
                  .Select(_ => "https://github.com/codehubapp/codehub")
                  .Subscribe(this.PresentSafari));
            });
        }
    }
}


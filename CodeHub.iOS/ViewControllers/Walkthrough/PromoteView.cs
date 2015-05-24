using UIKit;
using CodeHub.iOS.Services;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class PromoteView : UIViewController
    {
        public PromoteView()
            : base("PromoteView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var shouldStar = false;
            var shouldWatch = false;

            StarButton.BackgroundColor = UIColor.FromRGB(0x2c, 0x3e, 0x50);
            StarButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            StarButton.Layer.CornerRadius = 6f;
            StarButton.TouchUpInside += (sender, e) => {
                shouldStar = !shouldStar;
                DefaultValueService.Instance.Set("SHOULD_STAR_CODEHUB", shouldStar);
                var color = shouldStar ? UIColor.FromRGB(0xbd, 0xc3, 0xc7) : UIColor.FromRGB(0x2c, 0x3e, 0x50);
                StarButton.SetTitle(shouldStar ? "Good Choice!" : "Star", UIControlState.Normal);
                UIView.Animate(0.3f, () => StarButton.BackgroundColor = color);
                Alert();
            };

            WatchButton.BackgroundColor = UIColor.FromRGB(0x7f, 0x8c, 0x8d);
            WatchButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            WatchButton.Layer.CornerRadius = 6f;
            WatchButton.TouchUpInside += (sender, e) => {
                shouldWatch = !shouldWatch;
                DefaultValueService.Instance.Set("SHOULD_WATCH_CODEHUB", shouldWatch);
                var color = shouldWatch ? UIColor.FromRGB(0xbd, 0xc3, 0xc7) : UIColor.FromRGB(0x7f, 0x8c, 0x8d);
                WatchButton.SetTitle(shouldWatch ? "Very Nice!" : "Watch", UIControlState.Normal);
                UIView.Animate(0.3f, () => WatchButton.BackgroundColor = color);
                Alert();
            };
        }

        private bool _hasAlerted;
        private void Alert()
        {
            if (_hasAlerted)
                return;
            _hasAlerted = true;

            var alert = UIAlertController.Create("Awesome!", "When you login to your account I will make the changes! Thanks for helping create a better CodeHub!", UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, x => {}));
            PresentViewController(alert, true, null);
        }
    }
}


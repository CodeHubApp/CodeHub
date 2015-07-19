using UIKit;
using CodeHub.iOS.ViewControllers.App;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class GoProViewController : UIViewController
    {
        public GoProViewController()
            : base("GoProViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TellMeMoreButton.BackgroundColor = UIColor.FromRGB(0x27, 0xae, 0x60);
            TellMeMoreButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            TellMeMoreButton.Layer.CornerRadius = 6f;
            TellMeMoreButton.TouchUpInside += (_, __) => {
                var view = new UpgradeViewController();
                view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, 
                    (s, e) => DismissViewController(true, null));
                PresentViewController(new ThemedNavigationController(view), true, null);
            };
        }
    }
}


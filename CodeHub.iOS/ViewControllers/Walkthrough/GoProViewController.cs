using UIKit;
using System;
using CodeHub.iOS.ViewControllers.Application;
using MvvmCross.Platform;
using CodeHub.Core.Services;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class GoProViewController : BaseViewController
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

            OnActivation(d => d(TellMeMoreButton.GetClickedObservable().Subscribe(_ => TellMeMore())));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var features = Mvx.Resolve<IFeaturesService>();
            if (features.IsProEnabled)
            {
                TitleLabel.Text = "Pro Enabled!";
                DescriptionLabel.Text = "Thank you for your continued support! The following Pro features have been activated for your device:\n\n• Private Repositories\n• Enterprise Support\n• Push Notifications";
            }
        }

        private void TellMeMore()
        {
            var view = new UpgradeViewController();
            view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Images.Buttons.CancelButton };
            view.NavigationItem.LeftBarButtonItem.GetClickedObservable().Subscribe(_ => DismissViewController(true, null));
            PresentViewController(new ThemedNavigationController(view), true, null);
        }
    }
}


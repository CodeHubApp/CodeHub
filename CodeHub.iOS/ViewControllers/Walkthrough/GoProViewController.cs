using UIKit;
using CodeHub.iOS.ViewControllers.App;
using System;

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

            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            var isPro = appDelegate?.IsPro ?? false;

            OnActivation(d => d(TellMeMoreButton.GetClickedObservable().Subscribe(_ => TellMeMore())));

            if (isPro)
            {
                TitleLabel.Text = "Pro Enabled!";
                DescriptionLabel.Text = "Thanks for your continued support! The following Pro features have been activated for your device:\n\n• Private Repositories\n• Enterprise Support\n• Push Notifications";
            }
        }

        private void TellMeMore()
        {
            var view = new UpgradeViewController();
            view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Images.Cancel };
            view.OnActivation(d => d(view.NavigationItem.LeftBarButtonItem.GetClickedObservable()
                .Subscribe(_ => view.DismissViewController(true, null))));
            PresentViewController(new ThemedNavigationController(view), true, null);
        }
    }
}


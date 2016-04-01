using System;
using CoreGraphics;
using UIKit;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using CodeHub.iOS.ViewControllers.Application;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public class NewAccountViewController : BaseViewController
    {
        private readonly UIColor DotComBackgroundColor = UIColor.FromRGB(239, 239, 244);
        private readonly UIColor EnterpriseBackgroundColor = UIColor.FromRGB(50, 50, 50);

        public NewAccountViewController()
        {
            Title = "New Account";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var dotComButton = new AccountButton("GitHub.com", Images.Logos.DotComMascot);
            var enterpriseButton = new AccountButton("Enterprise", Images.Logos.EnterpriseMascot);

            dotComButton.BackgroundColor = DotComBackgroundColor;
            dotComButton.Label.TextColor = EnterpriseBackgroundColor;
            dotComButton.ImageView.TintColor = EnterpriseBackgroundColor;

            enterpriseButton.BackgroundColor = EnterpriseBackgroundColor;
            enterpriseButton.Label.TextColor = dotComButton.BackgroundColor;
            enterpriseButton.ImageView.TintColor = dotComButton.BackgroundColor;

            Add(dotComButton);
            Add(enterpriseButton);

            View.ConstrainLayout(() => 
                dotComButton.Frame.Top == View.Frame.Top &&
                dotComButton.Frame.Left == View.Frame.Left &&
                dotComButton.Frame.Right == View.Frame.Right &&
                dotComButton.Frame.Bottom == View.Frame.GetMidY() &&

                enterpriseButton.Frame.Top == dotComButton.Frame.Bottom &&
                enterpriseButton.Frame.Left == View.Frame.Left &&
                enterpriseButton.Frame.Right == View.Frame.Right &&
                enterpriseButton.Frame.Bottom == View.Frame.Bottom);

            OnActivation(d =>
            {
                d(dotComButton.GetClickedObservable().Subscribe(_ => DotComButtonTouch()));
                d(enterpriseButton.GetClickedObservable().Subscribe(_ => EnterpriseButtonTouch()));
            });
        }

        private void DotComButtonTouch ()
        {
            NavigationController.PushViewController(new LoginViewController(), true);
        }

        private void EnterpriseButtonTouch ()
        {
            var features = Mvx.Resolve<IFeaturesService>();
            if (features.IsProEnabled)
            {
                NavigationController.PushViewController(new AddAccountViewController(), true);
            }
            else
            {
                this.PresentUpgradeViewController();
            }
        }

        private class AccountButton : UIButton
        {
            public UILabel Label { get; private set; }

            public new UIImageView ImageView { get; private set; }

            public AccountButton(string text, UIImage image)
                : base(new CGRect(0, 0, 100, 100))
            {
                Label = new UILabel(new CGRect(0, 0, 100, 100));
                Label.Text = text;
                Label.TextAlignment = UITextAlignment.Center;
                Add(Label);

                ImageView = new UIImageView(new CGRect(0, 0, 100, 100));
                ImageView.Image = image;
                ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                Add(ImageView);

                this.ConstrainLayout(() => 
                    ImageView.Frame.Top == this.Frame.Top + 30f &&
                    ImageView.Frame.Left == this.Frame.Left &&
                    ImageView.Frame.Right == this.Frame.Right &&
                    ImageView.Frame.Bottom == this.Frame.Bottom - 60f &&

                    Label.Frame.Top == ImageView.Frame.Bottom + 10f &&
                    Label.Frame.Left == this.Frame.Left &&
                    Label.Frame.Right == this.Frame.Right &&
                    Label.Frame.Height == 20);
            }
        }
    }
}
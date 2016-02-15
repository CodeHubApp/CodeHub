using CoreGraphics;
using CodeHub.Core.ViewModels.Accounts;
using UIKit;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using CodeHub.iOS.Views.App;
using MvvmCross.iOS.Views;
using CodeHub.iOS.ViewControllers.Application;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public class NewAccountViewController : MvxViewController
    {
        private readonly UIColor DotComBackgroundColor = UIColor.FromRGB(239, 239, 244);
        private readonly UIColor EnterpriseBackgroundColor = UIColor.FromRGB(50, 50, 50);
        private AccountButton _dotComButton;
        private AccountButton _enterpriseButton;

        public NewAccountViewController()
        {
            Title = "New Account";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var dotComButton = _dotComButton = new AccountButton("GitHub.com", Images.Logos.DotComMascot);
            var enterpriseButton = _enterpriseButton = new AccountButton("Enterprise", Images.Logos.EnterpriseMascot);

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
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _enterpriseButton.TouchUpInside += EnterpriseButtonTouch;
            _dotComButton.TouchUpInside += DotComButtonTouch;
        }

        private void DotComButtonTouch (object sender, System.EventArgs e)
        {
            var viewModel = ViewModel as NewAccountViewModel;
            viewModel?.GoToDotComLoginCommand.Execute(null);
        }

        private void EnterpriseButtonTouch (object sender, System.EventArgs e)
        {
            var features = Mvx.Resolve<IFeaturesService>();
            if (features.IsEnterpriseSupportActivated)
                ((NewAccountViewModel)ViewModel).GoToEnterpriseLoginCommand.Execute(null);
            else
            {
                this.PresentUpgradeViewController();
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _enterpriseButton.TouchUpInside -= EnterpriseButtonTouch;
            _dotComButton.TouchUpInside -= DotComButtonTouch;
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
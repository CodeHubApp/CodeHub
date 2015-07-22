using CoreGraphics;
using CodeHub.Core.ViewModels.Accounts;
using UIKit;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public class NewAccountViewController : BaseViewController<NewAccountViewModel>
    {
        private readonly AccountButton _dotComButton = new AccountButton("GitHub.com", Images.Logos.GitHub);
        private readonly AccountButton _enterpriseButton = new AccountButton("Enterprise", Images.Logos.Enterprise);
        private readonly UIColor DotComBackgroundColor = UIColor.FromRGB(239, 239, 244);
        private readonly UIColor EnterpriseBackgroundColor = UIColor.FromRGB(50, 50, 50);

        public NewAccountViewController()
        {
            _dotComButton.BackgroundColor = DotComBackgroundColor;
            _dotComButton.Label.TextColor = EnterpriseBackgroundColor;
            _dotComButton.ImageView.TintColor = EnterpriseBackgroundColor;
            _dotComButton.TouchUpInside += (sender, e) => ViewModel.GoToDotComLoginCommand.ExecuteIfCan();

            _enterpriseButton.BackgroundColor = EnterpriseBackgroundColor;
            _enterpriseButton.Label.TextColor = _dotComButton.BackgroundColor;
            _enterpriseButton.ImageView.TintColor = _dotComButton.BackgroundColor;
            _enterpriseButton.TouchUpInside += (sender, e) => ViewModel.GoToEnterpriseLoginCommand.ExecuteIfCan();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Add(_dotComButton);
            Add(_enterpriseButton);

            View.ConstrainLayout(() => 
                _dotComButton.Frame.Top == View.Frame.Top &&
                _dotComButton.Frame.Left == View.Frame.Left &&
                _dotComButton.Frame.Right == View.Frame.Right &&
                _dotComButton.Frame.Bottom == View.Frame.GetMidY() &&

                _enterpriseButton.Frame.Top == _dotComButton.Frame.Bottom &&
                _enterpriseButton.Frame.Left == View.Frame.Left &&
                _enterpriseButton.Frame.Right == View.Frame.Right &&
                _enterpriseButton.Frame.Bottom == View.Frame.Bottom);
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


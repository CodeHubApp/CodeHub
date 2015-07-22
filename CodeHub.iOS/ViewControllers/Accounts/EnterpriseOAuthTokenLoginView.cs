using System;
using UIKit;
using CodeHub.Core.ViewModels.Accounts;
using ReactiveUI;
using CoreGraphics;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public partial class EnterpriseOAuthTokenLoginView : BaseViewController<EnterpriseOAuthTokenLoginViewModel>
    {
        private readonly UIColor BackgroundColor = UIColor.FromRGB(50, 50, 50);
        private readonly UIColor ComponentTextColor = UIColor.White;
        private readonly UIColor ComponentPlaceholderColor = UIColor.FromWhiteAlpha(0.8f, 1f);
        private readonly UIColor ComponentBackgroundColor = UIColor.FromRGB(130, 130, 130);

        public EnterpriseOAuthTokenLoginView()
            : base("EnterpriseOAuthTokenLoginView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var scopes = string.Join(", ", OctokitClientFactory.Scopes);
            DescriptionLabel.Text = string.Format("The provided Personal Access Token must allow access to the following scopes: {0}", scopes);
            DescriptionLabel.TextColor = ComponentTextColor;

            TokenText.AttributedPlaceholder = new Foundation.NSAttributedString("Token", foregroundColor: ComponentPlaceholderColor);
            TokenText.BackgroundColor = ComponentBackgroundColor;
            TokenText.TextColor = ComponentTextColor;
            TokenText.EditingChanged += (sender, args) => ViewModel.Token = TokenText.Text;
            ViewModel.WhenAnyValue(x => x.Token).Subscribe(x => TokenText.Text = x);

            DomainText.AttributedPlaceholder = new Foundation.NSAttributedString("Domain", foregroundColor: ComponentPlaceholderColor);
            DomainText.BackgroundColor = ComponentBackgroundColor;
            DomainText.TextColor = ComponentTextColor;
            DomainText.EditingChanged += (sender, args) => ViewModel.Domain = DomainText.Text;
            ViewModel.WhenAnyValue(x => x.Domain).Subscribe(x => DomainText.Text = x);

            LoginButton.TouchUpInside += (sender, args) => ViewModel.LoginCommand.ExecuteIfCan();
            ViewModel.LoginCommand.CanExecuteObservable.Subscribe(x => LoginButton.Enabled = x);

            View.BackgroundColor = BackgroundColor;
            LogoImageView.Image = Images.Logos.Enterprise;

            LoginButton.SetTitleColor(ComponentTextColor, UIControlState.Normal);
            LoginButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

            //Set some generic shadowing
            LoginButton.Layer.ShadowColor = UIColor.Black.CGColor;
            LoginButton.Layer.ShadowOffset = new CGSize(0, 1);
            LoginButton.Layer.ShadowOpacity = 0.2f;

            DomainText.ShouldReturn = delegate {
                TokenText.BecomeFirstResponder();
                return true;
            };

            TokenText.ShouldReturn = delegate {
                TokenText.ResignFirstResponder();
                LoginButton.SendActionForControlEvents(UIControlEvent.TouchUpInside);
                return true;
            };

            this.ViewportObservable().Subscribe(x => ScrollView.Frame = x);

            ImageHeight.Constant = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 192 : 86;
        }
    }
}


using System;
using UIKit;
using CodeHub.Core.ViewModels.Accounts;
using ReactiveUI;
using CoreGraphics;

namespace CodeHub.iOS.Views.Accounts
{
    public partial class EnterpriseOAuthTokenLoginView : BaseViewController<EnterpriseOAuthTokenLoginViewModel>
    {
        public EnterpriseOAuthTokenLoginView()
            : base("EnterpriseOAuthTokenLoginView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            TokenText.EditingChanged += (sender, args) => ViewModel.Token = TokenText.Text;
            ViewModel.WhenAnyValue(x => x.Token).Subscribe(x => TokenText.Text = x);

            DomainText.EditingChanged += (sender, args) => ViewModel.Domain = DomainText.Text;
            ViewModel.WhenAnyValue(x => x.Domain).Subscribe(x => DomainText.Text = x);

            LoginButton.TouchUpInside += (sender, args) => ViewModel.LoginCommand.ExecuteIfCan();
            ViewModel.LoginCommand.CanExecuteObservable.Subscribe(x => LoginButton.Enabled = x);

            View.BackgroundColor = UIColor.FromRGB(239, 239, 244);
            LogoImageView.Image = Images.Logos.GitHub;

            LoginButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            LoginButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

            //Set some generic shadowing
            LoginButton.Layer.ShadowColor = UIColor.Black.CGColor;
            LoginButton.Layer.ShadowOffset = new CGSize(0, 1);
            LoginButton.Layer.ShadowOpacity = 0.3f;

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
        }
    }
}


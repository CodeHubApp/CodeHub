using System;
using CoreGraphics;
using CodeHub.Core.ViewModels.Accounts;
using UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.Accounts
{
    public partial class AddAccountView : BaseViewController<AddWebAccountViewModel>
    {
        public AddAccountView()
            : base("AddAccountView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            User.EditingChanged += (sender, args) => ViewModel.Username = User.Text;
            ViewModel.WhenAnyValue(x => x.Username).Subscribe(x => User.Text = x);

            Password.EditingChanged += (sender, args) => ViewModel.Password = Password.Text;
            ViewModel.WhenAnyValue(x => x.Password).Subscribe(x => Password.Text = x);

            LoginButton.TouchUpInside += (sender, args) => ViewModel.LoginCommand.ExecuteIfCan();
            ViewModel.LoginCommand.CanExecuteObservable.Subscribe(x => LoginButton.Enabled = x);

			View.BackgroundColor = UIColor.FromRGB(239, 239, 244);
            Logo.Image = Images.Logos.GitHub;

            LoginButton.SetBackgroundImage(Images.Buttons.GreyButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

            //Set some generic shadowing
            LoginButton.Layer.ShadowColor = UIColor.Black.CGColor;
            LoginButton.Layer.ShadowOffset = new CGSize(0, 1);
            LoginButton.Layer.ShadowOpacity = 0.3f;

            User.ShouldReturn = delegate {
                Password.BecomeFirstResponder();
                return true;
            };
            Password.ShouldReturn = delegate {
                Password.ResignFirstResponder();
                LoginButton.SendActionForControlEvents(UIControlEvent.TouchUpInside);
                return true;
            };

            this.ViewportObservable().Subscribe(x => ScrollView.Frame = x);
        }
    }
}


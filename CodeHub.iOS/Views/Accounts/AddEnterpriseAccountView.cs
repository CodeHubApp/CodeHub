using System;
using UIKit;
using CodeHub.Core.ViewModels.Accounts;
using ReactiveUI;
using CoreGraphics;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Accounts
{
    public partial class AddEnterpriseAccountView : BaseViewController<AddEnterpriseAccountViewModel>
    {
        public AddEnterpriseAccountView()
            : base("AddEnterpriseAccountView", null)
        {
            this.WhenAnyValue(x => x.ViewModel.ShowLoginOptionsCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }
  
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Username.EditingChanged += (sender, args) => 
                ViewModel.Username = Username.Text;
            ViewModel.WhenAnyValue(x => x.Username).Subscribe(x => Username.Text = x);

            Password.EditingChanged += (sender, args) => 
                ViewModel.Password = Password.Text;
            ViewModel.WhenAnyValue(x => x.Password).Subscribe(x => Password.Text = x);

            Domain.EditingChanged += (sender, args) => 
                ViewModel.Domain = Domain.Text;
            ViewModel.WhenAnyValue(x => x.Domain).Subscribe(x => Domain.Text = x);

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

            Domain.ShouldReturn = delegate {
                Username.BecomeFirstResponder();
                return true;
            };

            Username.ShouldReturn = delegate {
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


using System;
using UIKit;
using CodeHub.Core.ViewModels.Accounts;
using ReactiveUI;
using CoreGraphics;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public partial class AddEnterpriseAccountView : BaseViewController<AddEnterpriseAccountViewModel>
    {
        private readonly UIColor BackgroundColor = UIColor.FromRGB(50, 50, 50);
        private readonly UIColor ComponentTextColor = UIColor.White;
        private readonly UIColor ComponentPlaceholderColor = UIColor.FromWhiteAlpha(0.8f, 1f);
        private readonly UIColor ComponentBackgroundColor = UIColor.FromRGB(130, 130, 130);

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

            Username.BackgroundColor = ComponentBackgroundColor;
            Username.AttributedPlaceholder = new Foundation.NSAttributedString("Username", foregroundColor: ComponentPlaceholderColor);
            Username.TextColor = ComponentTextColor;
            Username.EditingChanged += (sender, args) => 
                ViewModel.Username = Username.Text;
            ViewModel.WhenAnyValue(x => x.Username).Subscribe(x => Username.Text = x);

            Password.BackgroundColor = ComponentBackgroundColor;
            Password.AttributedPlaceholder = new Foundation.NSAttributedString("Password", foregroundColor: ComponentPlaceholderColor);
            Password.TextColor = ComponentTextColor;
            Password.EditingChanged += (sender, args) => 
                ViewModel.Password = Password.Text;
            ViewModel.WhenAnyValue(x => x.Password).Subscribe(x => Password.Text = x);

            Domain.BackgroundColor = ComponentBackgroundColor;
            Domain.AttributedPlaceholder = new Foundation.NSAttributedString("Domain", foregroundColor: ComponentPlaceholderColor);
            Domain.TextColor = ComponentTextColor;
            Domain.EditingChanged += (sender, args) => 
                ViewModel.Domain = Domain.Text;
            ViewModel.WhenAnyValue(x => x.Domain).Subscribe(x => Domain.Text = x);

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

            ImageHeight.Constant = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 192 : 86;
        }
    }
}


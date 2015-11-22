using System;
using UIKit;
using CodeHub.Core.ViewModels.Accounts;
using ReactiveUI;
using CoreGraphics;
using System.Reactive.Disposables;

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
        }
  
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Username.BackgroundColor = ComponentBackgroundColor;
            Username.AttributedPlaceholder = new Foundation.NSAttributedString("Username", foregroundColor: ComponentPlaceholderColor);
            Username.TextColor = ComponentTextColor;

            Password.BackgroundColor = ComponentBackgroundColor;
            Password.AttributedPlaceholder = new Foundation.NSAttributedString("Password", foregroundColor: ComponentPlaceholderColor);
            Password.TextColor = ComponentTextColor;

            Domain.BackgroundColor = ComponentBackgroundColor;
            Domain.AttributedPlaceholder = new Foundation.NSAttributedString("Domain", foregroundColor: ComponentPlaceholderColor);
            Domain.TextColor = ComponentTextColor;

            View.BackgroundColor = BackgroundColor;
            LogoImageView.Image = Images.Logos.Enterprise;

            LoginButton.SetTitleColor(ComponentTextColor, UIControlState.Normal);
            LoginButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

            //Set some generic shadowing
            LoginButton.Layer.ShadowColor = UIColor.Black.CGColor;
            LoginButton.Layer.ShadowOffset = new CGSize(0, 1);
            LoginButton.Layer.ShadowOpacity = 0.2f;

            this.ViewportObservable().Subscribe(x => ScrollView.Frame = x);

            ImageHeight.Constant = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 192 : 86;

            OnActivation(d => {
                d(Username.GetChangedObservable().Subscribe(x => ViewModel.Username = x));
                d(this.WhenAnyValue(x => x.ViewModel.Username).Subscribe(x => Username.Text = x));
                d(Password.GetChangedObservable().Subscribe(x => ViewModel.Password = x));
                d(this.WhenAnyValue(x => x.ViewModel.Password).Subscribe(x => Password.Text = x));
                d(Domain.GetChangedObservable().Subscribe(x => ViewModel.Domain = x));
                d(this.WhenAnyValue(x => x.ViewModel.Domain).Subscribe(x => Domain.Text = x));
                d(LoginButton.GetClickedObservable().InvokeCommand(ViewModel.LoginCommand));
                d(ViewModel.LoginCommand.CanExecuteObservable.Subscribe(x => LoginButton.Enabled = x));

                d(this.WhenAnyValue(x => x.ViewModel.ShowLoginOptionsCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Action, x => NavigationItem.RightBarButtonItem = x));

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

                d(Disposable.Create(() => {
                    Domain.ShouldReturn = null;
                    Username.ShouldReturn = null;
                    Password.ShouldReturn = null;
                }));
            });
        }
    }
}


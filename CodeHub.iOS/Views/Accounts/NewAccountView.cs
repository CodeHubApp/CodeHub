using CoreGraphics;
using CodeHub.Core.ViewModels.Accounts;
using UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.Accounts
{
    public partial class NewAccountView : BaseViewController<NewAccountViewModel>
    {
        public NewAccountView()
			: base ("NewAccountView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			View.BackgroundColor = UIColor.FromRGB(239, 239, 244);
			
            InternetButton.SetBackgroundImage(Images.Buttons.GreyButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            EnterpriseButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

            InternetButton.Layer.ShadowColor = UIColor.Black.CGColor;
            InternetButton.Layer.ShadowOffset = new CGSize(0, 1);
            InternetButton.Layer.ShadowOpacity = 0.3f;
            InternetButton.TouchUpInside += (sender, e) => ViewModel.GoToDotComLoginCommand.ExecuteIfCan();

            EnterpriseButton.Layer.ShadowColor = UIColor.Black.CGColor;
            EnterpriseButton.Layer.ShadowOffset = new CGSize(0, 1);
            EnterpriseButton.Layer.ShadowOpacity = 0.3f;
            EnterpriseButton.TouchUpInside += (sender, e) => ViewModel.GoToEnterpriseLoginCommand.ExecuteIfCan();

            ScrollView.ContentSize = new CGSize(View.Bounds.Width, EnterpriseButton.Frame.Bottom + 10f);
        }
    }
}


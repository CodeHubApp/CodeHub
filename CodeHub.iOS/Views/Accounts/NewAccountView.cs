using System.Drawing;
using CodeHub.Core.ViewModels.Accounts;
using MonoTouch.UIKit;
using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.iOS.Views.Accounts
{
    public partial class NewAccountView : BaseViewController<NewAccountViewModel>
    {
        private readonly IFeaturesService _featuresService;

        public NewAccountView(IFeaturesService featuresService)
			: base ("NewAccountView", null)
        {
            _featuresService = featuresService;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			View.BackgroundColor = UIColor.FromRGB(239, 239, 244);
			
            InternetButton.SetBackgroundImage(Images.Buttons.GreyButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            EnterpriseButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

            InternetButton.Layer.ShadowColor = UIColor.Black.CGColor;
            InternetButton.Layer.ShadowOffset = new SizeF(0, 1);
            InternetButton.Layer.ShadowOpacity = 0.3f;
            InternetButton.TouchUpInside += (sender, e) => ViewModel.GoToDotComLoginCommand.ExecuteIfCan();

            EnterpriseButton.Layer.ShadowColor = UIColor.Black.CGColor;
            EnterpriseButton.Layer.ShadowOffset = new SizeF(0, 1);
            EnterpriseButton.Layer.ShadowOpacity = 0.3f;

            EnterpriseButton.TouchUpInside += (sender, e) => GoToEnterprise();

            ScrollView.ContentSize = new SizeF(View.Bounds.Width, EnterpriseButton.Frame.Bottom + 10f);
        }

        private void GoToEnterprise()
        {
            if (_featuresService.IsEnterpriseSupportActivated)
                ViewModel.GoToEnterpriseLoginCommand.Execute(null);
            else
            {
//                var ctrl = IoC.Resolve<EnableEnterpriseViewController>();
//                ctrl.Dismissed += (sender, e) => {
//                    if (_featuresService.IsEnterpriseSupportActivated)
//                        ViewModel.GoToEnterpriseLoginCommand.Execute(null);
//                };
//                PresentViewController(ctrl, true, null);
            }
        }
    }
}


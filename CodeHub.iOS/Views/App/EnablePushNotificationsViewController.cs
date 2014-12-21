using System;
using System.Drawing;
using MonoTouch.UIKit;
using CodeHub.Core.Services;
using Xamarin.Utilities.Services;

namespace CodeHub.iOS.Views.App
{
    public partial class EnablePushNotificationsViewController : UIViewController
    {
        private readonly IStatusIndicatorService _statusIndicatorService;
        private readonly IFeaturesService _featuresService;

        public EnablePushNotificationsViewController(IStatusIndicatorService statusIndicatorService, IFeaturesService featuresService) 
            : base("EnablePushNotificationsViewController", null)
        {
            _statusIndicatorService = statusIndicatorService;
            _featuresService = featuresService;
        }

        public event EventHandler Dismissed;

        private void OnDismissed()
        {
            var handler = Dismissed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.View.AutosizesSubviews = true;

            ImageView.Image = UIImageHelper.FromFileAuto("iTunesArtwork");
            ImageView.Layer.CornerRadius = 24f;
            ImageView.Layer.MasksToBounds = true;

            CancelButton.SetBackgroundImage(Images.Buttons.GreyButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            CancelButton.TintColor = UIColor.Black;
            CancelButton.Layer.ShadowColor = UIColor.Black.CGColor;
            CancelButton.Layer.ShadowOffset = new SizeF(0, 1);
            CancelButton.Layer.ShadowOpacity = 0.3f;
            CancelButton.TouchUpInside += (sender, e) => DismissViewController(true, OnDismissed);

            EnableButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            EnableButton.TintColor = UIColor.White;
            EnableButton.Layer.ShadowColor = UIColor.Black.CGColor;
            EnableButton.Layer.ShadowOffset = new SizeF(0, 1);
            EnableButton.Layer.ShadowOpacity = 0.3f;
            EnableButton.TouchUpInside += EnablePushNotifications;

            PushLabel.Layer.CornerRadius = PushLabel.Frame.Width / 2;
            PushLabel.Layer.MasksToBounds = true;
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            ContainerView.Frame = new RectangleF(View.Bounds.Width / 2 - ContainerView.Frame.Width / 2, 
                                                 View.Bounds.Height / 2 - ContainerView.Frame.Height / 2, 
                                                 ContainerView.Frame.Width, ContainerView.Frame.Height);
        }

        void HandlePurchaseError (object sender, Exception e)
        {
            _statusIndicatorService.Hide();
        }

        void HandlePurchaseSuccess (object sender, string e)
        {
            _statusIndicatorService.Hide();
            DismissViewController(true, OnDismissed);
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                return UIInterfaceOrientationMask.Portrait;
            return base.GetSupportedInterfaceOrientations();
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                return UIInterfaceOrientation.Portrait;
            return base.PreferredInterfaceOrientationForPresentation();
        }

        private void EnablePushNotifications(object sender, EventArgs e)
        {
            _statusIndicatorService.Show("Enabling...");
            _featuresService.Activate(FeatureIds.PushNotifications);
        }
    }
}


using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.CrossCore;
using CodeHub.Core.Services;
using CodeFramework.iOS.Utils;
using CodeFramework.Core.Services;

namespace CodeHub.iOS.Views.App
{
    public partial class EnablePushNotificationsViewController : UIViewController
    {
        private IHud _hud;

        public EnablePushNotificationsViewController() 
            : base("EnablePushNotificationsViewController", null)
        {
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

            ImageView.Image = UIImageHelper.FromFileAuto("iTunesArtwork");
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

            var lbl = new UILabel();
            lbl.Frame = new RectangleF(ImageView.Frame.Width - 25, -15, 40, 40);
            lbl.TextAlignment = UITextAlignment.Center;
            lbl.Layer.CornerRadius = lbl.Frame.Width / 2;
            lbl.Layer.MasksToBounds = true;
            lbl.BackgroundColor = UIColor.Red;
            lbl.Text = "12";
            lbl.Font = UIFont.SystemFontOfSize(18f);
            lbl.TextColor = UIColor.White;

            ImageView.AddSubview(lbl);

            _hud = new Hud(View);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            InAppPurchases.Instance.PurchaseSuccess += HandlePurchaseSuccess;
            InAppPurchases.Instance.PurchaseError += HandlePurchaseError;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            InAppPurchases.Instance.PurchaseSuccess -= HandlePurchaseSuccess;
            InAppPurchases.Instance.PurchaseError -= HandlePurchaseError;
        }

        void HandlePurchaseError (object sender, Exception e)
        {
            _hud.Hide();
        }

        void HandlePurchaseSuccess (object sender, string e)
        {
            _hud.Hide();
            DismissViewController(true, OnDismissed);
        }

        private void EnablePushNotifications(object sender, EventArgs e)
        {
            _hud.Show("Enabling...");
            var featureService = Mvx.Resolve<IFeaturesService>();
            featureService.Activate(InAppPurchases.PushNotificationsId);
        }
    }
}


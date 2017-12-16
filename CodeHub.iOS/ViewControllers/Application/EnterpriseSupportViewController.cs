using System;
using UIKit;
using System.Reactive.Linq;
using MessageUI;
using CodeHub.Core.Services;
using Splat;
using Foundation;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class EnterpriseSupportViewController : BaseViewController
    {
        private static string CodeHubUrl = "https://github.com/CodeHubApp/CodeHub";

        private readonly IAlertDialogService _alertDialogService;
        private readonly UIColor ComponentColor = UIColor.FromWhiteAlpha(0.9f, 1f);
        private NSLayoutConstraint[] _defaultConstraints;
        private NSLayoutConstraint[] _landscapeConstraints;

        public EnterpriseSupportViewController(IAlertDialogService alertDialogService = null)
        {
            _alertDialogService = alertDialogService ?? Locator.Current.GetService<IAlertDialogService>();

            Appearing
                .Select(_ => NavigationController)
                .Where(x => x != null)
                .Subscribe(x => x.NavigationBar.ShadowImage = new UIImage());
        }

        private void SubmitFeedback()
        {
            if (!MFMailComposeViewController.CanSendMail)
            {
                _alertDialogService.Alert(
                    "No Email Setup",
                    "Looks like you don't have email setup on this device. " +
                    "Add a mail provider and try again.").ToBackground();
            }
            else
            {
                var ctrl = new MFMailComposeViewController();
                ctrl.SetSubject("CodeHub Support");
                ctrl.SetToRecipients(new[] { "codehubapp@gmail.com" });
                ctrl.Finished += (sender, e) => DismissViewController(true, () =>
                {
                    if (e.Result == MFMailComposeResult.Sent)
                        _alertDialogService.Alert("Sent!", "Thanks for your feedback!");
                });
                PresentViewController(ctrl, true, null);
            }
        }

        private void GoToGitHub()
        {
            var viewController = new WebBrowserViewController(CodeHubUrl);
            PresentViewController(viewController, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.FromRGB(50, 50, 50);

            var imageView = new UIImageView(Octicon.Heart.ToImage(256, false));
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            imageView.TintColor = UIColor.White;
            View.Add(imageView);

            var label = new UILabel();
            label.Text = "Or";
            label.TextColor = ComponentColor;
            label.TextAlignment = UITextAlignment.Center;
            View.Add(label);

            var webButton = new UIButton(UIButtonType.Custom);
            webButton.SetTitle("CodeHub on GitHub", UIControlState.Normal);
            webButton.TouchUpInside += (sender, e) => GoToGitHub();

            var button = new UIButton(UIButtonType.Custom);
            button.SetTitle("Email Me!", UIControlState.Normal);
            button.TouchUpInside += (sender, e) => SubmitFeedback();

            foreach (var b in new[] { webButton, button })
            {
                button.SetTitleColor(ComponentColor, UIControlState.Normal);
                b.Font = UIFont.PreferredBody;
                b.Layer.BorderColor = label.TextColor.CGColor;
                b.Layer.BorderWidth = 1;
                b.Layer.CornerRadius = 6f;
                View.Add(b);
            }

            _defaultConstraints = View.ConstrainLayout(() =>
                button.Frame.Width == 212 &&
                button.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                button.Frame.Top == imageView.Frame.Bottom + 20f &&
                button.Frame.Height == 44 &&

                imageView.Frame.Width == 192 &&
                imageView.Frame.Height == 192 &&
                imageView.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                imageView.Frame.GetCenterY() == View.Frame.GetCenterY() - 96f &&

                label.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                label.Frame.Top == button.Frame.Bottom + 10 &&
                label.Frame.Width == 192 &&
                label.Frame.Height == 44 &&

                webButton.Frame.Width == button.Frame.Width &&
                webButton.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                webButton.Frame.Top == label.Frame.Bottom + 10f &&
                webButton.Frame.Height == 44);


            _landscapeConstraints = View.ConstrainLayout(() =>
                button.Frame.Width == 212 &&
                button.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                button.Frame.Top == imageView.Frame.Bottom + 20f &&
                button.Frame.Height == 44 &&

                imageView.Frame.Width == 32 &&
                imageView.Frame.Height == 32 &&
                imageView.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                imageView.Frame.GetCenterY() == View.Frame.GetCenterY() - 96f &&

                label.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                label.Frame.Top == button.Frame.Bottom + 10 &&
                label.Frame.Width == 100 &&
                label.Frame.Height == 44 &&

                webButton.Frame.Width == button.Frame.Width &&
                webButton.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                webButton.Frame.Top == label.Frame.Bottom + 10f &&
                webButton.Frame.Height == 44,
                false);
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            base.WillRotate(toInterfaceOrientation, duration);

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                if (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)
                {
                    View.RemoveConstraints(_landscapeConstraints);
                    View.AddConstraints(_defaultConstraints);
                    View.SetNeedsUpdateConstraints();
                    View.UpdateConstraintsIfNeeded();
                }
                else
                {
                    View.RemoveConstraints(_defaultConstraints);
                    View.AddConstraints(_landscapeConstraints);
                    View.SetNeedsUpdateConstraints();
                    View.UpdateConstraintsIfNeeded();
                }
            }
        }
    }
}


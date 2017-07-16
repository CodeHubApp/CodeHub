
using System;
using CodeHub.Core.Services;
using UIKit;
using System.Threading.Tasks;
using Foundation;
using CoreGraphics;
using System.Collections.Generic;
using System.Linq;
using BigTed;

namespace CodeHub.iOS.Services
{
    public class AlertDialogService : IAlertDialogService
    {
        public Task<bool> PromptYesNo(string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = new UIAlertView {Title = title, Message = message};
            alert.CancelButtonIndex = alert.AddButton("No");
            var ok = alert.AddButton("Yes");
            alert.Clicked += (sender, e) => tcs.TrySetResult(e.ButtonIndex == ok);
            alert.Show();
            return tcs.Task;
        }

        public Task Alert(string title, string message)
        {
            var tcs = new TaskCompletionSource<object>();
            ShowAlert(title, message, () => tcs.TrySetResult(null));
            return tcs.Task;
        }

        public static void ShowAlert(string title, string message, Action dismissed = null)
        {
            var window = new UIWindow(UIScreen.MainScreen.Bounds);
            window.RootViewController = new UIViewController();

            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, x => {
                dismissed?.Invoke();
                alert.Dispose();
                window.Dispose();
            }));

            var topWindow = UIApplication.SharedApplication.Windows.Last();
            window.WindowLevel = topWindow.WindowLevel + 1;

            window.MakeKeyAndVisible();
            window.RootViewController.PresentViewController(alert, true, null);
        }

        public static void Share(string title = null, string body = null, string url = null, UIBarButtonItem barButtonItem = null)
        {
            try
            {
                var activityItems = new List<NSObject>();
                if (body != null)
                    activityItems.Add(new NSString(body));
                if (url != null)
                    activityItems.Add(new NSUrl(url));

                UIActivity[] applicationActivities = null;
                var activityController = new UIActivityViewController (activityItems.ToArray(), applicationActivities);

                if (title != null)
                activityController.SetValueForKey(new NSString(title), new NSString("subject"));

                if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) 
                {
                    var window = UIApplication.SharedApplication.KeyWindow;
                    var pop = new UIPopoverController (activityController);

                    if (barButtonItem != null)
                    {
                        pop.PresentFromBarButtonItem(barButtonItem, UIPopoverArrowDirection.Any, true);
                    }
                    else
                    {
                        var rect = new CGRect(window.RootViewController.View.Frame.Width / 2, window.RootViewController.View.Frame.Height / 2, 0, 0);
                        pop.PresentFromRect (rect, window.RootViewController.View, UIPopoverArrowDirection.Any, true);
                    }
                } 
                else 
                {
                    var viewController = UIApplication.SharedApplication.KeyWindow.GetVisibleViewController();
                    viewController.PresentViewController(activityController, true, null);
                }
            }
            catch
            {
            }
        }

        public Task<string> PromptTextBox(string title, string message, string defaultValue, string okTitle)
        {
            var tcs = new TaskCompletionSource<string>();
            var alert = new UIAlertView();
            alert.Title = title;
            alert.Message = message;
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            var cancelButton = alert.AddButton("Cancel");
            var okButton = alert.AddButton(okTitle);
            alert.CancelButtonIndex = cancelButton;
            alert.GetTextField(0).Text = defaultValue;
            alert.Clicked += (s, e) =>
            {
                if (e.ButtonIndex == okButton)
                    tcs.SetResult(alert.GetTextField(0).Text);
                else
                    tcs.SetCanceled();
            };
            alert.Show();
            return tcs.Task;
        }

        public static UIColor BackgroundTint;

        public void Show(string text)
        {
            ProgressHUD.Shared.HudBackgroundColour = BackgroundTint;
            BTProgressHUD.Show(text, maskType: ProgressHUD.MaskType.Gradient);
            UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
        }

        public void ShowSuccess(string text)
        {
            BTProgressHUD.ShowSuccessWithStatus(text);
        }

        public void ShowError(string text)
        {
            BTProgressHUD.ShowErrorWithStatus(text);
        }

        public void Hide()
        {
            BTProgressHUD.Dismiss();
            UIApplication.SharedApplication.EndIgnoringInteractionEvents();
        }
    }
}


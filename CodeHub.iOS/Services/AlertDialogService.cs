
using System;
using CodeHub.Core.Services;
using UIKit;
using System.Threading.Tasks;
using Foundation;
using CoreGraphics;

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
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, x => {
                dismissed?.Invoke();
                alert.Dispose();
            }));
            UIApplication.SharedApplication.KeyWindow.GetVisibleViewController().PresentViewController(alert, true, null);
        }

        public static void ShareUrl(string url, UIBarButtonItem barButtonItem = null)
        {
            try
            {
                var item = new NSUrl(url);
                var activityItems = new NSObject[] { item };
                UIActivity[] applicationActivities = null;
                var activityController = new UIActivityViewController (activityItems, applicationActivities);

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
    }
}


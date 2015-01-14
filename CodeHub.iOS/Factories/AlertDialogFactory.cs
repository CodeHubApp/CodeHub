using System.Threading.Tasks;
using MonoTouch.UIKit;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.Factories
{
    public class AlertDialogFactory : IAlertDialogFactory
    {
        // ReSharper disable once NotAccessedField.Local
        private UIAlertView _alertView;

        public Task<bool> PromptYesNo(string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = _alertView = new UIAlertView { Title = title, Message = message };
            alert.CancelButtonIndex = alert.AddButton("No");
            var ok = alert.AddButton("Yes");
            alert.Clicked += (sender, e) =>
            {
                tcs.SetResult(e.ButtonIndex == ok);
                _alertView = null;
            };
            alert.Show();
            return tcs.Task;
        }

        public Task Alert(string title, string message)
        {
            var tcs = new TaskCompletionSource<object>();
            var alert = _alertView = new UIAlertView { Title = title, Message = message };
            alert.DismissWithClickedButtonIndex(alert.AddButton("Ok"), true);
            alert.Dismissed += (sender, e) =>
            {
                tcs.SetResult(null);
                _alertView = null;
            };
            alert.Show();
            return tcs.Task;
        }

        public Task<string> PromptTextBox(string title, string message, string defaultValue, string okTitle)
        {
            var tcs = new TaskCompletionSource<string>();
            var alert = _alertView = new UIAlertView();
            alert.Title = title;
            alert.Message = message;
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            var cancelButton = alert.AddButton("Cancel");
            var okButton = alert.AddButton(okTitle);
            alert.CancelButtonIndex = cancelButton;
            alert.DismissWithClickedButtonIndex(cancelButton, true);
            alert.GetTextField(0).Text = defaultValue;
            alert.Clicked += (s, e) =>
            {
                if (e.ButtonIndex == okButton)
                    tcs.SetResult(alert.GetTextField(0).Text);
                else
                    tcs.SetCanceled();
                _alertView = null;
            };
            alert.Show();
            return tcs.Task;
        }
    }
}


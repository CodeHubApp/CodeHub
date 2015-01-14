using BigTed;
using MonoTouch.UIKit;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Services
{
    public class StatusIndicatorService : IStatusIndicatorService
    {
        public static UIColor BackgroundTint;

		public void Show(string text)
		{
			ProgressHUD.Shared.HudBackgroundColour = BackgroundTint;
			BTProgressHUD.Show(text, maskType: ProgressHUD.MaskType.Gradient);
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
		}
    }
}
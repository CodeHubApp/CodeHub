using BigTed;
using UIKit;
using System;
using System.Reactive.Disposables;

namespace CodeHub.iOS.Utilities
{
    public class Hud : IHud
    {
        public static UIColor BackgroundTint;
        public Hud(UIView window)
        {
        }

        public void Show(string text)
        {
            ProgressHUD.Shared.HudBackgroundColour = BackgroundTint;
            BTProgressHUD.Show(text, maskType: ProgressHUD.MaskType.Gradient);
        }

        public static void ShowSuccess(string text)
        {
            ProgressHUD.Shared.HudBackgroundColour = BackgroundTint;
            BTProgressHUD.ShowSuccessWithStatus(text);
        }

        public static void ShowFailure(string text)
        {
            ProgressHUD.Shared.HudBackgroundColour = BackgroundTint;
            BTProgressHUD.ShowErrorWithStatus(text);
        }

        public void Hide()
        {
            BTProgressHUD.Dismiss();
        }
    }

    public static class HudExtensions
    {
        public static IDisposable SubscribeStatus(this IObservable<bool> @this, string message)
        {
            var hud = new Hud(null);
            var isShown = false;

            var d = @this.Subscribe(x => {
                if (x && !isShown)
                    hud.Show(message);
                else if (!x && isShown)
                    hud.Hide();
                isShown = x;
            }, err => {
                BTProgressHUD.ShowErrorWithStatus(err.Message);
                isShown = false;
            }, () => {
                if (isShown)
                    BTProgressHUD.Dismiss();
            });

            var d2 = Disposable.Create(() =>
            {
                if (isShown)
                    BTProgressHUD.Dismiss();
            });

            return new CompositeDisposable(d, d2);
        }

        public static IDisposable Activate(this IHud hud, string text)
        {
            hud.Show(text);
            return Disposable.Create(hud.Hide);
        }
    }

    public interface IHud
    {
        void Show(string text);

        void Hide();
    }
}


using System;
using Foundation;
using UIKit;
using System.Reactive.Subjects;
using CoreGraphics;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using Splat;

namespace CodeHub.iOS.Views
{
    public static class BaseViewControllerExtensions
    {
        public static IAnalyticsService GetAnalytics(this BaseViewController viewController)
        {
            return Locator.Current.GetService<IAnalyticsService>();
        }

        public static void TrackScreen(this BaseViewController viewController)
        {
            var analytics = viewController.GetAnalytics();
            if (analytics == null)
                return;

            var screenName = viewController.GetType().Name;
            analytics.Screen(screenName);
        }

        public static IObservable<CGRect> ViewportObservable(this BaseViewController viewController)
        {
            NSObject hideNotification = null;
            NSObject showNotification = null;
            var subject = new Subject<CGRect>();

            var notificationAction = new Action<NSNotification>(notification =>
            {
                if (viewController.IsViewLoaded) 
                {

                    //Start an animation, using values from the keyboard
                    UIView.BeginAnimations ("AnimateForKeyboard");
                    UIView.SetAnimationBeginsFromCurrentState (true);
                    UIView.SetAnimationDuration (UIKeyboard.AnimationDurationFromNotification (notification));
                    UIView.SetAnimationCurve ((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification (notification));

                    //Pass the notification, calculating keyboard height, etc.
                    var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.FrameBeginUserInfoKey) as NSValue;
                    if (nsValue != null) 
                    {
                        var kbSize = nsValue.RectangleFValue.Size;
                        var view = viewController.View.Bounds;
                        view.Height = view.Height - kbSize.Height;
                        subject.OnNext(view);
                    }

                    //Commit the animation
                    UIView.CommitAnimations (); 
                }
            });

            viewController.Appearing
                .Subscribe(_ =>
                {
                    hideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, notificationAction);
                    showNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, notificationAction);
                });

            viewController.Disappearing
                .Where(_ => showNotification != null && hideNotification != null)
                .Subscribe(_ =>
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(hideNotification);
                    NSNotificationCenter.DefaultCenter.RemoveObserver(showNotification);
                });

            return subject;
        }
    }
}


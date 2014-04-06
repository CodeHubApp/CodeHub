using System;
using CodeHub.Core.Factories;
using CodeHub.iOS.Views.App;
using System.Threading.Tasks;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Factory
{
    public class FeatureFactory : IFeatureFactory
    {
        public Task PromptPushNotificationFeature()
        {
            var tcs = new TaskCompletionSource<object>();
            var ctrl = new EnablePushNotificationsViewController();
            ctrl.Dismissed += (sender, e) => tcs.SetResult(null);
            (UIApplication.SharedApplication.Delegate as AppDelegate).Window.RootViewController.PresentViewController(ctrl, true, null);
            return tcs.Task;
        }
    }
}


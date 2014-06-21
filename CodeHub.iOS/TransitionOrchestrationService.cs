using System;
using CodeFramework.iOS.Views.Application;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.iOS
{
    class TransitionOrchestrationService : ITransitionOrchestrationService
    {
        public void Transition(IViewFor fromView, IViewFor toView)
        {
            var fromViewController = (UIViewController)fromView;
            var fromViewModel = (IBaseViewModel)fromView.ViewModel;
            var toViewController = (UIViewController)toView;
            var toViewModel = (IBaseViewModel)toView.ViewModel;

            fromViewController.BeginInvokeOnMainThread(() => DoTransition(fromViewController, fromViewModel, toViewController, toViewModel));
        }

        private void DoTransition(UIViewController fromViewController, IBaseViewModel fromViewModel, UIViewController toViewController, IBaseViewModel toViewModel)
        {
            var toViewDismissCommand = toViewModel.DismissCommand;

//            if (toViewController is SettingsViewController)
//            {
//                toViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(CodeFramework.iOS.Images.Cancel, UIBarButtonItemStyle.Plain, (s, e) => toViewDismissCommand.ExecuteIfCan());
//                toViewDismissCommand.Subscribe(__ => toViewController.DismissViewController(true, null));
//                fromViewController.PresentViewController(new UINavigationController(toViewController), true, null);
//            }
//            else if (toViewController is AccountsView)
//            {
//                var rootNav = (UINavigationController)UIApplication.SharedApplication.Delegate.Window.RootViewController;
//                toViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(CodeFramework.iOS.Images.Cancel, UIBarButtonItemStyle.Plain, (s, e) => toViewDismissCommand.ExecuteIfCan());
//                toViewDismissCommand.Subscribe(_ => rootNav.DismissViewController(true, null));
//                rootNav.PresentViewController(new UINavigationController(toViewController), true, null);
//            }
//            else if (fromViewController is RepositoriesViewController)
//            {
//                fromViewController.NavigationController.PresentViewController(toViewController, true, null);
//            }
//            else if (toViewController is MainViewController)
//            {
//                var nav = ((UINavigationController)UIApplication.SharedApplication.Delegate.Window.RootViewController);
//                UIView.Transition(nav.View, 0.1, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.TransitionCrossDissolve,
//                    () => nav.PushViewController(toViewController, false), null);
//            }
//            else if (toViewController is LoginViewController && fromViewController is StartupView)
//            {
//                toViewDismissCommand.Subscribe(_ => toViewController.DismissViewController(true, null));
//                fromViewController.PresentViewController(new UINavigationController(toViewController), true, null);
//            }
//            else if (fromViewController is MainViewController)
//            {
//                var slideout = ((MainViewController)fromViewController);
//                slideout.MainViewController = new MainNavigationController(toViewController, slideout,
//                    new UIBarButtonItem(CodeFramework.iOS.Images.MenuButton, UIBarButtonItemStyle.Plain, (s, e) => slideout.Open(true)));
//            }
//            else
            {
                toViewDismissCommand.Subscribe(_ => toViewController.NavigationController.PopToViewController(fromViewController, true));
                fromViewController.NavigationController.PushViewController(toViewController, true);
            }
        }

    }
}
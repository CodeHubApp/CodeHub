using System;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.Views.Accounts;
using CodeHub.iOS.Views.App;
using MonoTouch.SlideoutNavigation;
using CodeHub.iOS.Views.Repositories;
using RepositoryStumble.Transitions;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views.Gists;
using CodeHub.iOS.Views.Source;
using Xamarin.Utilities.Services;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Views;
using MonoTouch.Foundation;
using Splat;
using CodeHub.iOS.Views.Contents;

namespace CodeHub.iOS
{
    class TransitionOrchestrationService : ITransitionOrchestrationService, IEnableLogger
    {
        private static NSObject NSObject = new NSObject();
        private readonly IViewModelViewService _viewModelViewService;
        private readonly IServiceConstructor _serviceConstructor;

        public TransitionOrchestrationService(IViewModelViewService viewModelViewService, IServiceConstructor serviceConstructor)
        {
            _viewModelViewService = viewModelViewService;
            _serviceConstructor = serviceConstructor;
        }

        public void Transition(IViewFor fromView, IViewFor toView)
        {
            NSObject.BeginInvokeOnMainThread(() => DoTransition(fromView, toView));
        }

        private void DoTransition(IViewFor fromView, IViewFor toView)
        {
            var toViewController = (UIViewController)toView;
            var toViewModel = (IBaseViewModel)toView.ViewModel;
            var fromViewController = (UIViewController)fromView;
            var routableToViewModel = toViewModel as Xamarin.Utilities.ViewModels.IRoutableViewModel;
            IReactiveCommand<object> toViewDismissCommand = null;


            if (toViewController is AccountsView || toViewController is WebBrowserView || toViewController is GistCommentView ||
                toViewController is CommitCommentView || toViewController is GistCreateView || toViewController is FeedbackComposerView)
            {
                var rootNav = (UINavigationController)UIApplication.SharedApplication.Delegate.Window.RootViewController;
                toViewDismissCommand = ReactiveCommand.Create().WithSubscription(_ => rootNav.DismissViewController(true, null));
                toViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Plain, (s, e) => toViewDismissCommand.ExecuteIfCan());
                var navController = new ThemedNavigationController(toViewController);
                rootNav.PresentViewController(navController, true, null);
            }
            else if (toViewController is MenuView)
            {
                var nav = ((UINavigationController)UIApplication.SharedApplication.Delegate.Window.RootViewController);
                var slideout = new SlideoutNavigationController();
                slideout.MenuViewController = new MenuNavigationController(toViewController, slideout);
                UIView.Transition(nav.View, 0.1, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.TransitionCrossDissolve,
                    () => nav.PushViewController(slideout, false), null);
            }
            else if (toViewController is NewAccountView && fromViewController is StartupView)
            {
                toViewDismissCommand = ReactiveCommand.Create().WithSubscription(_ => toViewController.DismissViewController(true, null));
                fromViewController.PresentViewController(new ThemedNavigationController(toViewController), true, null);
            }
            else if (fromViewController is MenuView)
            {
                fromViewController.NavigationController.PushViewController(toViewController, true);
            }
            else if (toViewController is LanguagesView && fromViewController is RepositoriesTrendingView)
            {
                toViewDismissCommand = ReactiveCommand.Create().WithSubscription(_ => fromViewController.DismissViewController(true, null));
                toViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done, (s, e) => toViewDismissCommand.ExecuteIfCan());
                var ctrlToPresent = new ThemedNavigationController(toViewController);
                ctrlToPresent.TransitioningDelegate = new SlideDownTransition();
                fromViewController.PresentViewController(ctrlToPresent, true, null);
            }
            else if (toViewController is EditFileView || toViewController is CreateFileView)
            {
                toViewDismissCommand = ReactiveCommand.Create().WithSubscription(_ => fromViewController.DismissViewController(true, null));
                toViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Plain, (s, e) => toViewDismissCommand.ExecuteIfCan());
                fromViewController.PresentViewController(new ThemedNavigationController(toViewController), true, null);
            }
            else
            {
                toViewDismissCommand = ReactiveCommand.Create().WithSubscription(_ => toViewController.NavigationController.PopToViewController(fromViewController, true));
                fromViewController.NavigationController.PushViewController(toViewController, true);
            }

            if (toViewDismissCommand != null)
            {
                routableToViewModel.RequestDismiss.Subscribe(_ => 
                {
                    this.Log().Info("{0} is requesting dismissal", routableToViewModel.GetType().Name);
                    toViewDismissCommand.ExecuteIfCan();
                });
            }

            toViewModel.RequestNavigation.Subscribe(x =>
            {
                var viewType = _viewModelViewService.GetViewFor(x.GetType());
                var view = (IViewFor)_serviceConstructor.Construct(viewType);
                view.ViewModel = x;
                Transition(toView, view);
            });
        }
    }
}
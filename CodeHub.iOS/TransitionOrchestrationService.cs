using System;
using UIKit;
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
using Foundation;
using Splat;
using CodeHub.iOS.Views.Contents;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Views;
using CodeHub.iOS.Views.Issues;
using System.Reactive;
using System.Threading.Tasks;

namespace CodeHub.iOS
{
    class TransitionOrchestrationService : ITransitionOrchestrationService, IEnableLogger
    {
        private readonly static NSObject ThreadObject = new NSObject();
        private readonly IViewModelViewService _viewModelViewService;
        private readonly IServiceConstructor _serviceConstructor;

        public TransitionOrchestrationService(IViewModelViewService viewModelViewService, IServiceConstructor serviceConstructor)
        {
            _viewModelViewService = viewModelViewService;
            _serviceConstructor = serviceConstructor;
        }

        public void Transition(IViewFor fromView, IViewFor toView)
        {
            ThreadObject.BeginInvokeOnMainThread(() => DoTransition(fromView, toView));
        }

        private void DoTransition(IViewFor fromView, IViewFor toView)
        {
            var toViewController = (UIViewController)toView;
            var toViewModel = (IBaseViewModel)toView.ViewModel;
            var fromViewController = (UIViewController)fromView;
            var routableToViewModel = toViewModel as IRoutingViewModel;
            IReactiveCommand<Unit> toViewDismissCommand = null;


            if (toViewController is AccountsView || toViewController is WebBrowserView || toViewController is GistCommentView ||
                toViewController is CommitCommentView || toViewController is GistCreateView || toViewController is FeedbackComposerView || 
                toViewController is IssueAddView)
            {
                var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
                var rootNav = (UINavigationController)appDelegate.Window.RootViewController;
                toViewDismissCommand = ReactiveCommand.CreateAsyncTask(_ => rootNav.DismissViewControllerAsync(true));
                toViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Plain, (s, e) => toViewDismissCommand.ExecuteIfCan());
                var navController = new ThemedNavigationController(toViewController);
                rootNav.PresentViewController(navController, true, null);
            }
            else if (toViewController is MenuView)
            {
                var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
                var nav = ((UINavigationController)appDelegate.Window.RootViewController);
                var slideout = new SlideoutNavigationController();
                slideout.MenuViewController = new MenuNavigationController(toViewController, slideout);
                UIView.Transition(nav.View, 0.3, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.TransitionCrossDissolve,
                    () => nav.PushViewController(slideout, false), null);
            }
            else if (toViewController is NewAccountView && fromViewController is StartupView)
            {
                toViewDismissCommand = ReactiveCommand.CreateAsyncTask(_ => toViewController.DismissViewControllerAsync(true));
                fromViewController.PresentViewController(new ThemedNavigationController(toViewController), true, null);
            }
            else if (fromViewController is MenuView)
            {
                fromViewController.NavigationController.PushViewController(toViewController, true);
            }
            else if (toViewController is LanguagesView && fromViewController is RepositoriesTrendingView)
            {
                toViewDismissCommand = ReactiveCommand.CreateAsyncTask(_ => fromViewController.DismissViewControllerAsync(true));
                toViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done, (s, e) => toViewDismissCommand.ExecuteIfCan());
                var ctrlToPresent = new ThemedNavigationController(toViewController);
                ctrlToPresent.TransitioningDelegate = new SlideDownTransition();
                fromViewController.PresentViewController(ctrlToPresent, true, null);
            }
            else if (toViewController is EditFileView || toViewController is CreateFileView)
            {
                toViewDismissCommand = ReactiveCommand.CreateAsyncTask(_ => fromViewController.DismissViewControllerAsync(true));

                toViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Plain, (s, e) => toViewDismissCommand.ExecuteIfCan());
                fromViewController.PresentViewController(new ThemedNavigationController(toViewController), true, null);
            }
            else
            {
                toViewDismissCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                    toViewController.NavigationController.PopToViewController(fromViewController, true);
                });

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
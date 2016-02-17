using System;
using MvvmCross.Platform;
using MvvmCross.Core.ViewModels;
using UIKit;
using CodeHub.Core;
using MonoTouch.SlideoutNavigation;
using MvvmCross.iOS.Views.Presenters;
using MvvmCross.iOS.Views;

namespace CodeHub.iOS
{
    public class IosViewPresenter : MvxBaseIosViewPresenter
    {
        private readonly UIWindow _window;
        private UINavigationController _generalNavigationController;
        private IMvxModalIosView _currentModal;

        public SlideoutNavigationController SlideoutNavigationController { get; set; }

        public IosViewPresenter(UIWindow window)
        {
            _window = window;
        }

        public override void ChangePresentation(MvxPresentationHint hint)
        {
            var closeHint = hint as MvxClosePresentationHint;
            if (closeHint != null)
            {
                if (_currentModal != null)
                {
                    ((UIViewController)_currentModal).DismissViewController(true, null);
                    return;
                }

                for (int i = _generalNavigationController.ViewControllers.Length - 1; i >= 1; i--)
                {
                    var vc = _generalNavigationController.ViewControllers[i];
                    var touchView = vc as IMvxIosView;
                    if (touchView != null && touchView.ViewModel == closeHint.ViewModelToClose)
                    {
                        _generalNavigationController.PopToViewController(_generalNavigationController.ViewControllers[i - 1], true);
                        return;
                    }
                }

                //If it didnt trigger above it's because it was probably the root.
                _generalNavigationController.PopToRootViewController(true);
            }
        }

        public override void Show(MvxViewModelRequest request)
        {
            var viewCreator = Mvx.Resolve<IMvxIosViewCreator>();
            var view = viewCreator.CreateView(request);
            var uiView = view as UIViewController;

            if (uiView == null)
                throw new InvalidOperationException("Asking to show a view which is not a UIViewController!");

            if (uiView is IMvxModalIosView)
            {
                _currentModal = (IMvxModalIosView)uiView;
                var modalNavigationController = new UINavigationController(uiView);
                modalNavigationController.NavigationBar.Translucent = false;
                modalNavigationController.Toolbar.Translucent = false;
                uiView.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.CancelButton, UIBarButtonItemStyle.Plain, (s, e) =>
                {
                    var vm = ((IMvxModalIosView)uiView).ViewModel;
                    Mvx.Resolve<MvvmCross.Plugins.Messenger.IMvxMessenger>().Publish(new CodeHub.Core.Messages.CancelationMessage(vm));
                    modalNavigationController.DismissViewController(true, null);
                    _currentModal = null;
                });
                PresentModalViewController(modalNavigationController, true);
            }
            else
            {
                if (request.PresentationValues != null && request.PresentationValues.ContainsKey(PresentationValues.SlideoutRootPresentation))
                {
                    var openButton = new UIBarButtonItem { Image = Theme.CurrentTheme.ThreeLinesButton };
                    var mainNavigationController = new MainNavigationController(uiView, SlideoutNavigationController, openButton);
                    _generalNavigationController = mainNavigationController;
                    SlideoutNavigationController.SetMainViewController(mainNavigationController, true);
                }
                else
                {
                    _generalNavigationController.PushViewController(uiView, true);
                }
            }
        }

        public override bool PresentModalViewController(UIViewController viewController, bool animated)
        {
            if (_window.RootViewController == null)
                return false;
            _window.RootViewController.PresentViewController(viewController, true, null);
            return true;
        }
    }
}

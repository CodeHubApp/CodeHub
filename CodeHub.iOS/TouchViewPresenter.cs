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
                _window?.GetVisibleViewController()?.PresentViewController(uiView, true, null);
                return;
            }

            if (request.PresentationValues != null && request.PresentationValues.ContainsKey(PresentationValues.SlideoutRootPresentation))
            {
                var openButton = new UIBarButtonItem { Image = Images.Buttons.ThreeLinesButton };
                var mainNavigationController = new MainNavigationController(uiView, SlideoutNavigationController, openButton);
                _generalNavigationController = mainNavigationController;
                SlideoutNavigationController.SetMainViewController(mainNavigationController, true);
            }
            else
            {
                _generalNavigationController.PushViewController(uiView, true);
            }
        }

        public override bool PresentModalViewController(UIViewController viewController, bool animated)
        {
            _window?.GetVisibleViewController()?.PresentViewController(viewController, true, null);
            return true;
        }
    }
}

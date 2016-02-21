using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public class WelcomePageViewController : BaseViewController
    {
        private readonly CardPageViewController[] _pages;
        private readonly UIButton _nextButton = new UIButton(UIButtonType.Custom);
        private readonly WelcomeViewController _welcomeViewController = new WelcomeViewController();
        private readonly UIPageViewController _welcomePageController = 
            new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal);
        
        public event Action WantsToDimiss;

        protected void OnWantsToDismiss() => WantsToDimiss?.Invoke();

        private IEnumerable<UIViewController> GetPages()
        {
            yield return new AboutViewController();
            yield return new PromoteViewController();
            yield return new OrgViewController();
            yield return new GoProViewController();
            yield return new FeedbackViewController();
            yield return _welcomeViewController;
        }

        public WelcomePageViewController()
        {
            _pages = GetPages().Select(x => new CardPageViewController(x)).ToArray();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Theme.CurrentTheme.PrimaryColor;

            _welcomePageController.DataSource = new PageDataSource(_pages);
            _welcomePageController.SetViewControllers(new [] { _pages[0] }, UIPageViewControllerNavigationDirection.Forward, true, null);
            _welcomePageController.View.Frame = new CoreGraphics.CGRect(0, 0, View.Frame.Width, View.Frame.Height);
            _welcomePageController.View.AutoresizingMask = UIViewAutoresizing.All;
            AddChildViewController(_welcomePageController);
            Add(_welcomePageController.View);

            _nextButton.SetTitle("Next", UIControlState.Normal);
            _nextButton.TintColor = UIColor.White;
            _nextButton.TitleLabel.Font = UIFont.SystemFontOfSize(14f);
            _nextButton.Frame = new CoreGraphics.CGRect(View.Frame.Width - 50f, View.Frame.Height - 28f, 40f, 20f);
            _nextButton.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleTopMargin;
            Add(_nextButton);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _nextButton.TouchUpInside += GoToNext;
            _welcomePageController.WillTransition += WillTransition;
            _welcomeViewController.WantsToDimiss += OnWantsToDismiss;

            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Fade);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _nextButton.TouchUpInside -= GoToNext;
            _welcomePageController.WillTransition -= WillTransition;
            _welcomeViewController.WantsToDimiss -= OnWantsToDismiss;
        }

        private void Transition(UIViewController e)
        {
            var isLast = _pages.Last() == e;
            //nextButton.Enabled = !isLast;
            UIView.Transition(_nextButton, 0.25f, UIViewAnimationOptions.TransitionCrossDissolve, 
                () => _nextButton.SetTitle(isLast ? "Done" : "Next", UIControlState.Normal), null);
        }

        private void WillTransition(object sender, UIPageViewControllerTransitionEventArgs args)
        {
            Transition(args.PendingViewControllers[0]);
        }

        private void GoToNext(object sender, EventArgs args)
        {
            var currentViewController = _welcomePageController.ViewControllers[0];
            var nextViewController = _welcomePageController.DataSource.GetNextViewController(_welcomePageController, currentViewController);
            if (nextViewController != null)
            {
                Transition(nextViewController);
                _welcomePageController.SetViewControllers(new [] { nextViewController }, UIPageViewControllerNavigationDirection.Forward, true, null);
            }
            else
            {
                OnWantsToDismiss();
            }
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone 
                ? UIInterfaceOrientationMask.Portrait : UIInterfaceOrientationMask.All;
        }

        public override bool ShouldAutorotate()
        {
            return UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone;
        }
            
        private class PageDataSource : UIPageViewControllerDataSource
        {
            private readonly IList<UIViewController> _pages;

            public PageDataSource(IEnumerable<UIViewController> pages)
            {
                _pages = pages.ToList();
            }

            public override nint GetPresentationCount(UIPageViewController pageViewController)
            {
                return _pages.Count;
            }

            public override nint GetPresentationIndex(UIPageViewController pageViewController)
            {
                return _pages.IndexOf(pageViewController.ViewControllers[0]);
            }

            public override UIViewController GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
            {
                var index = _pages.IndexOf(referenceViewController);
                return index == (_pages.Count - 1) ? null : _pages[index + 1];
            }

            public override UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
            {
                var index = _pages.IndexOf(referenceViewController);
                return index == 0 ? null : _pages[index - 1];
            }
        }
    }
}


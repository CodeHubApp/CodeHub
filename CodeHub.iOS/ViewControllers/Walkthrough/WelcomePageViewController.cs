using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public class WelcomePageViewController : UIViewController
    {
        public event Action WantsToDimiss;

        protected void OnWantsToDismiss()
        {
            var e = WantsToDimiss;
            if (e != null)
                e();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Theme.PrimaryNavigationBarColor;

            var welcomeViewController = new WelcomeViewController();
            welcomeViewController.WantsToDimiss += OnWantsToDismiss;

            var pages = new UIViewController[]
            { 
                new CardPageViewController(new AboutViewController()),
                new CardPageViewController(new PromoteView()),
                new CardPageViewController(new GoProViewController()),
                new CardPageViewController(new FeedbackViewController()),
                new CardPageViewController(welcomeViewController),
            };

            var welcomePageViewController = new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal);
            welcomePageViewController.DataSource = new PageDataSource(pages);
            welcomePageViewController.SetViewControllers(new [] { pages[0] }, UIPageViewControllerNavigationDirection.Forward, true, null);
            welcomePageViewController.View.Frame = new CoreGraphics.CGRect(0, 0, View.Frame.Width, View.Frame.Height);
            welcomePageViewController.View.AutoresizingMask = UIViewAutoresizing.All;
            AddChildViewController(welcomePageViewController);
            Add(welcomePageViewController.View);

            var nextButton = new UIButton(UIButtonType.Custom);
            nextButton.SetTitle("Next", UIControlState.Normal);
            nextButton.TintColor = UIColor.White;
            nextButton.TitleLabel.Font = UIFont.SystemFontOfSize(14f);
            nextButton.Frame = new CoreGraphics.CGRect(View.Frame.Width - 50f, View.Frame.Height - 28f, 40f, 20f);
            nextButton.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleTopMargin;
            Add(nextButton);

            var transitionAction = new Action<UIViewController>(e => {
                var isLast = pages.Last() == e;
                //nextButton.Enabled = !isLast;
                UIView.Transition(nextButton, 0.25f, UIViewAnimationOptions.TransitionCrossDissolve, 
                    () => nextButton.SetTitle(isLast ? "Done" : "Next", UIControlState.Normal), null);
            });

            welcomePageViewController.WillTransition += (sender, e) => transitionAction(e.PendingViewControllers[0]);

            nextButton.TouchUpInside += (sender, e) => {
                var currentViewController = welcomePageViewController.ViewControllers[0];
                var nextViewController = welcomePageViewController.DataSource.GetNextViewController(welcomePageViewController, currentViewController);
                if (nextViewController != null)
                {
                    transitionAction(nextViewController);
                    welcomePageViewController.SetViewControllers(new [] { nextViewController }, UIPageViewControllerNavigationDirection.Forward, true, null);
                }
                else
                {
                    // We're done here...
                }
            };
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


using System;
using UIKit;
using System.Reactive.Linq;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers.Search
{
    public class ExploreViewController : BaseViewController
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Repositories", "Users" });

        private Lazy<UIViewController> _repositoryViewController =
            new Lazy<UIViewController>(() => new RepositoryExploreViewController());

        private Lazy<UIViewController> _userViewController =
            new Lazy<UIViewController>(() => new UserExploreViewController());

        public ExploreViewController()
        {
            Title = "Explore";

            _viewSegment.SelectedSegment = 0;
            NavigationItem.TitleView = _viewSegment;
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "" };

            this.WhenActivated(d =>
            {
                d(_viewSegment.GetChangedObservable()
                  .StartWith((int)_viewSegment.SelectedSegment)
                  .Subscribe(HandleSegmentChange));
            });
        }

        private void HandleSegmentChange(int segmentId)
        {
            foreach (var childViewController in ChildViewControllers)
            {
                childViewController.RemoveFromParentViewController();
                childViewController.View.RemoveFromSuperview();
            }

            var newViewController = segmentId == 0 ? _repositoryViewController.Value : _userViewController.Value;
            AddChildViewController(newViewController);

            var newView = newViewController.View;
            newView.Frame = View.Bounds;
            newView.AutoresizingMask = UIViewAutoresizing.All;
            View.AddSubview(newView);
        }
    }
}


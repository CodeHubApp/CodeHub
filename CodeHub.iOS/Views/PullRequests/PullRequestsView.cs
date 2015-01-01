using System;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.UIKit;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : BaseTableViewController<PullRequestsViewModel>
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButtonItem;

        public PullRequestsView()
        {
            _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
            _segmentBarButtonItem = new UIBarButtonItem(_viewSegment);
            ToolbarItems = new[] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButtonItem, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

            this.WhenViewModel(x => x.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = x);
            _viewSegment.ValueChanged += (sender, args) => ViewModel.SelectedFilter = _viewSegment.SelectedSegment;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _segmentBarButtonItem.Width = View.Frame.Width - 10f;
            TableView.Source = new PullRequestTableViewSource(TableView, ViewModel.PullRequests);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (NavigationController != null)
                NavigationController.SetToolbarHidden(false, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (NavigationController != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}


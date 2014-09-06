using System;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : ReactiveTableViewController<PullRequestsViewModel>
    {
        private UISegmentedControl _viewSegment;
        private UIBarButtonItem _segmentBarButtonItem;

        public override void ViewDidLoad()
        {
            Title = "Pull Requests";

            base.ViewDidLoad();

            this.AddSearchBar(x => ViewModel.SearchKeyword = x);

            _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
            _segmentBarButtonItem = new UIBarButtonItem(_viewSegment) {Width = View.Frame.Width - 10f};
            ToolbarItems = new[] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButtonItem, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            _viewSegment.ValueChanged += (sender, args) => ViewModel.SelectedFilter = _viewSegment.SelectedSegment;
            ViewModel.WhenAnyValue(x => x.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = x);

            TableView.Source = new PullRequestTableViewSource(TableView, ViewModel.PullRequests);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}


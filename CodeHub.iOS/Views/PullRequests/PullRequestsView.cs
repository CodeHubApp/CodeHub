using System;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : ReactiveTableViewController<PullRequestsViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
        private readonly UIBarButtonItem _segmentBarButtonItem;

        public PullRequestsView()
        {
            _segmentBarButtonItem = new UIBarButtonItem(_viewSegment);
            ToolbarItems = new[] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButtonItem, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

            this.WhenViewModel(x => x.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = x);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _segmentBarButtonItem.Width = View.Frame.Width - 10f;
            _viewSegment.ValueChanged += (sender, args) => ViewModel.SelectedFilter = _viewSegment.SelectedSegment;
            TableView.Source = new PullRequestTableViewSource(TableView, ViewModel.PullRequests);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ToolbarItems != null && NavigationController != null)
                NavigationController.SetToolbarHidden(false, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null && NavigationController != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}


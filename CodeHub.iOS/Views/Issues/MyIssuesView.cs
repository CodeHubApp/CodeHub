using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Issues
{
    public class MyIssuesView : BaseTableViewController<MyIssuesViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed", "Custom" });
		private readonly UIBarButtonItem _segmentBarButton;

        public MyIssuesView()
        {
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.IssueOpened.ToImage(64f), "There are no issues."));

            this.WhenActivated(d =>
            {
                var valueChanged = Observable.FromEventPattern(x => _viewSegment.ValueChanged += x, x => _viewSegment.ValueChanged -= x);
                d(this.WhenAnyValue(x => x.ViewModel.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = x));
                d(valueChanged.Subscribe(_ => ViewModel.SelectedFilter = (int)_viewSegment.SelectedSegment));
                d(this.WhenAnyValue(x => x.ViewModel.GroupedIssues).IsNotNull().Subscribe(x =>
                {
                    var source = TableView.Source as IssueTableViewSource;
                    if (source != null) source.SetData(x);
                }));
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new IssueTableViewSource(TableView);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            _segmentBarButton.Width = View.Frame.Width - 10f;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _segmentBarButton.Width = View.Frame.Width - 10f;
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


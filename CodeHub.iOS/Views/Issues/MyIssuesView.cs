using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Issues
{
    public class MyIssuesView : NewReactiveTableViewController<MyIssuesViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed", "Custom" });
		private readonly UIBarButtonItem _segmentBarButton;

        public MyIssuesView()
        {
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

            this.WhenActivated(d =>
            {
                var valueChanged = Observable.FromEventPattern(x => _viewSegment.ValueChanged += x, x => _viewSegment.ValueChanged -= x);
                d(this.WhenAnyValue(x => x.ViewModel.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = x));
                d(valueChanged.Subscribe(_ => ViewModel.SelectedFilter = _viewSegment.SelectedSegment));
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


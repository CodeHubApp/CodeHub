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
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });

        public MyIssuesView()
        {
            NavigationItem.TitleView = _viewSegment;

            this.WhenAnyValue(x => x.ViewModel.GoToFilterCommand)
                .Select(x => x.ToBarButtonItem(Images.Filter))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.CustomFilterEnabled)
                .Where(_ => NavigationItem.RightBarButtonItem != null)
                .Subscribe(x => {
                    NavigationItem.RightBarButtonItem.Image = x ? Images.FilterFilled : Images.Filter;
                });

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.IssueOpened.ToEmptyListImage(), "There are no issues."));

            // Only attach once.
            this.WhenAnyValue(x => x.ViewModel.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = x);

            this.WhenActivated(d =>
            {
                var valueChanged = Observable.FromEventPattern(x => _viewSegment.ValueChanged += x, x => _viewSegment.ValueChanged -= x);
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
    }
}


using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class MyIssuesViewController : BaseTableViewController<MyIssuesViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });

        public MyIssuesViewController()
        {
            NavigationItem.TitleView = _viewSegment;

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.IssueOpened.ToEmptyListImage(), "There are no issues."));

            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.GoToFilterCommand)
                    .ToBarButtonItem(Images.Filter, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.CustomFilterEnabled)
                    .Where(_ => NavigationItem.RightBarButtonItem != null)
                    .Subscribe(x => NavigationItem.RightBarButtonItem.Image = x ? Images.FilterFilled : Images.Filter));

                d(_viewSegment.GetChangedObservable().Subscribe(x => ViewModel.SelectedFilter = x));

                d(this.WhenAnyValue(x => x.ViewModel.GroupedIssues).IsNotNull().Subscribe(x => {
                    var source = TableView.Source as IssueTableViewSource;
                    if (source != null) source.SetData(x);
                }));

                d(this.WhenAnyValue(x => x.ViewModel.SelectedFilter)
                    .Select(x => ViewModel.CustomFilterEnabled ? -1 : x)
                    .Subscribe(x => _viewSegment.SelectedSegment = x));
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new IssueTableViewSource(TableView);
        }
    }
}


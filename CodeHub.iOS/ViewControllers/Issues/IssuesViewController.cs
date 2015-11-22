using System;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Views;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using CodeHub.iOS.ViewControllers;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class IssuesViewController : BaseTableViewController<IssuesViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new [] { "Open", "Closed", "Mine" });

        public IssuesViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.IssueOpened.ToImage(64f), "There are no issues."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var filterBarButtonItem = new UIBarButtonItem { Image = Images.Filter };
            var addBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add);

            OnActivation(d => {
                d(filterBarButtonItem.GetClickedObservable().InvokeCommand(ViewModel.GoToFilterCommand));
                d(addBarButtonItem.GetClickedObservable().InvokeCommand(ViewModel.GoToNewIssueCommand));
                d(_viewSegment.GetChangedObservable().Subscribe(x => ViewModel.FilterSelection = (IssuesViewModel.IssueFilterSelection)x));
                d(this.WhenAnyValue(x => x.ViewModel.FilterSelection)
                    .Select(x => x == IssuesViewModel.IssueFilterSelection.Custom)
                    .Subscribe(x => {
                        filterBarButtonItem.Image = x ? Images.FilterFilled : Images.Filter;
                        if (x) _viewSegment.SelectedSegment = -1;
                    }));
            });

            NavigationItem.TitleView = _viewSegment;
            TableView.Source = new IssueTableViewSource(TableView, ViewModel.Items);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _viewSegment.SelectedSegment = (int)ViewModel.FilterSelection;
        }
    }
}


using System;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewComponents;
using ReactiveUI;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : BaseTableViewController<PullRequestsViewModel>
    {
        private readonly UISegmentedControl _viewSegment;

        public PullRequestsView()
        {
            _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
            NavigationItem.TitleView = _viewSegment;

            this.WhenAnyValue(x => x.ViewModel.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = x);
            _viewSegment.ValueChanged += (sender, args) => ViewModel.SelectedFilter = (int)_viewSegment.SelectedSegment;

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitPullRequest.ToImage(64f), "There are no pull requests."));

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new PullRequestTableViewSource(TableView, ViewModel.PullRequests);
        }
    }
}


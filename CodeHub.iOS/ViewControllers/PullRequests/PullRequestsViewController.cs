using System;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestsViewController : BaseTableViewController<PullRequestsViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitPullRequest.ToImage(64f), "There are no pull requests."));

            var viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
            this.WhenAnyValue(x => x.ViewModel.SelectedFilter).Subscribe(x => viewSegment.SelectedSegment = x);
            viewSegment.ValueChanged += (sender, args) => ViewModel.SelectedFilter = (int)viewSegment.SelectedSegment;
            NavigationItem.TitleView = viewSegment;

            this.WhenAnyValue(x => x.ViewModel.Items)
                .Select(x => new PullRequestTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}


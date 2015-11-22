using System;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestsViewController : BaseTableViewController<PullRequestsViewModel>
    {
        public PullRequestsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitPullRequest.ToImage(64f), "There are no pull requests.")); 
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
            NavigationItem.TitleView = viewSegment;

            TableView.Source = new PullRequestTableViewSource(TableView, ViewModel.Items);

            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.SelectedFilter).Subscribe(x => viewSegment.SelectedSegment = x));
                d(viewSegment.GetChangedObservable().Subscribe(x => ViewModel.SelectedFilter = x));
            });
        }
    }
}


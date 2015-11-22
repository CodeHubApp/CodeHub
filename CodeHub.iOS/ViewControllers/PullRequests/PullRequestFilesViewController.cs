using System;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.iOS.TableViewSources;
using UIKit;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestFilesViewController : BaseTableViewController<PullRequestFilesViewModel>
    {
        public PullRequestFilesViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileCode.ToEmptyListImage(), "There are no files."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new CommitedFilesTableViewSource(TableView, ViewModel.Files);
        }
    }
}




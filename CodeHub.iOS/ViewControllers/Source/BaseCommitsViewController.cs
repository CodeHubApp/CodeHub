using CodeHub.Core.ViewModels.Changesets;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using UIKit;
using System;

namespace CodeHub.iOS.ViewControllers.Source
{
    public abstract class BaseCommitsViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseCommitsViewModel
	{
        protected BaseCommitsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitCommit.ToEmptyListImage(), "There are no commits."));
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            TableView.Source = new CommitTableViewSource(TableView, ViewModel.Commits);
		}
	}
}


using CodeHub.Core.ViewModels.Changesets;
using CodeHub.iOS.TableViewSources;
using UIKit;
using System;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Source
{
    public abstract class BaseCommitsView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseCommitsViewModel
	{
        protected BaseCommitsView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitCommit.ToImage(64f), "There are no commits."));
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            TableView.Source = new CommitTableViewSource(TableView, ViewModel.Commits);
		}
	}
}


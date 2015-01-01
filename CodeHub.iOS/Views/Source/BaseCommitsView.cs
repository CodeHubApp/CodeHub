using CodeHub.Core.ViewModels.Changesets;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Source
{
    public abstract class BaseCommitsView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseCommitsViewModel
	{
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            TableView.Source = new CommitTableViewSource(TableView, ViewModel.Commits);
		}
	}
}


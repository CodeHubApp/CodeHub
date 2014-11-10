using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Source
{
    public abstract class CommitsView<TViewModel> : ReactiveTableViewController<TViewModel> where TViewModel : CommitsViewModel
	{
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            TableView.Source = new CommitTableViewSource(TableView, ViewModel.Commits);
		}
	}
}


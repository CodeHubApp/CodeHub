using CodeHub.Core.ViewModels.Changesets;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;
using System;

namespace CodeHub.iOS.Views.Source
{
    public abstract class BaseCommitsView<TViewModel> : NewReactiveTableViewController<TViewModel> where TViewModel : BaseCommitsViewModel
	{
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            TableView.Source = new CommitTableViewSource(TableView, ViewModel.Commits);
		}
	}
}


using CodeHub.Core.ViewModels.Changesets;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using UIKit;
using System;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Source
{
    public abstract class BaseCommitsViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseCommitsViewModel
	{
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitCommit.ToEmptyListImage(), "There are no commits."));

            this.WhenAnyValue(x => x.ViewModel.Items)
                .Select(x => new CommitTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
		}
	}
}


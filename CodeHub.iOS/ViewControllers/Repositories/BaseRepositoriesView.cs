using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.TableViewSources;
using UIKit;
using System;
using CodeHub.iOS.Views;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public abstract class BaseRepositoriesView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseRepositoriesViewModel
    {
        protected BaseRepositoriesView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Repo.ToEmptyListImage(), "There are no repositories."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.WhenAnyValue(x => x.ViewModel.Repositories)
                .Select(x => new RepositoryTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}
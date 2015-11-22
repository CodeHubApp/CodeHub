using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.TableViewSources;
using UIKit;
using System;
using CodeHub.iOS.Views;

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
            TableView.Source = new RepositoryTableViewSource(TableView, ViewModel.Items);
        }
    }
}
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Repositories
{
    public abstract class BaseRepositoriesView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseRepositoriesViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new RepositoryTableViewSource(TableView, ViewModel.Repositories);
        }
    }
}
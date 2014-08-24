using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Repositories
{
    public abstract class BaseRepositoriesView<TViewModel> : ReactiveTableViewController<TViewModel> where TViewModel : BaseRepositoriesViewModel
    {
        public override void ViewDidLoad()
        {
            Title = "Repositories";

            base.ViewDidLoad();

            TableView.Source = new RepositoryTableViewSource(TableView, ViewModel.Repositories);
            this.AddSearchBar(x => ViewModel.SearchKeyword = x);
        }
    }
}
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Repositories
{
    public abstract class BaseRepositoriesView<TViewModel> : NewReactiveTableViewController<TViewModel> where TViewModel : BaseRepositoriesViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new RepositoryTableViewSource(TableView, ViewModel.Repositories);
        }
    }
}
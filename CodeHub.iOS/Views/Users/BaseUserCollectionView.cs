using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Users
{
    public abstract class BaseUserCollectionView<TViewModel> : ReactiveTableViewController<TViewModel> where TViewModel : BaseUserCollectionViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new UserTableViewSource(TableView, ViewModel.Users);
        }
    }
}


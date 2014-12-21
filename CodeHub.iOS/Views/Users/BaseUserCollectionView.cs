using CodeHub.Core.ViewModels.Users;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Users
{
    public abstract class BaseUserCollectionView<TViewModel> : NewReactiveTableViewController<TViewModel> where TViewModel : BaseUserCollectionViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new UserTableViewSource(TableView, ViewModel.Users);
        }
    }
}


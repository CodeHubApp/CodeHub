using CodeHub.Core.ViewModels.Users;
using CodeHub.iOS.TableViewSources;
using UIKit;
using CodeHub.iOS.ViewComponents;
using System;

namespace CodeHub.iOS.Views.Users
{
    public abstract class BaseUserCollectionView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseUsersViewModel
    {
        protected BaseUserCollectionView(string emptyString)
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Person.ToImage(64f), emptyString ?? "There are no users."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new UserTableViewSource(TableView, ViewModel.Users);
        }
    }
}


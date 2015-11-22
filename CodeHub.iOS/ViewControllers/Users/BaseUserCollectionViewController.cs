using CodeHub.Core.ViewModels.Users;
using CodeHub.iOS.TableViewSources;
using UIKit;
using System;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Users
{
    public abstract class BaseUserCollectionViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseUsersViewModel
    {
        protected BaseUserCollectionViewController(string emptyString)
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Person.ToEmptyListImage(), emptyString ?? "There are no users."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new UserTableViewSource(TableView, ViewModel.Items);
        }
    }
}


using CodeHub.Core.ViewModels.Users;
using CodeHub.iOS.TableViewSources;
using UIKit;
using System;
using ReactiveUI;
using System.Reactive.Linq;
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

            this.WhenAnyValue(x => x.ViewModel.Items)
                .Select(x => new UserTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}


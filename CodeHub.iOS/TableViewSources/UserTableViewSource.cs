using ReactiveUI;
using CodeHub.Core.ViewModels.Users;
using CodeHub.iOS.Cells;

namespace CodeHub.iOS.TableViewSources
{
    public class UserTableViewSource : ReactiveTableViewSource<UserItemViewModel>
    {
        public UserTableViewSource(UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<UserItemViewModel> collection) 
            : base(tableView, collection,  UserTableViewCell.Key, UserTableViewCell.DefaultHeight)
        {
            tableView.RegisterNibForCellReuse(UserTableViewCell.Nib, UserTableViewCell.Key);
        }
    }
}


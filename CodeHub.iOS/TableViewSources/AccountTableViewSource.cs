using ReactiveUI;
using CodeHub.Core.ViewModels.Accounts;
using UIKit;
using CodeHub.iOS.Cells;

namespace CodeHub.iOS.TableViewSources
{
    public class AccountTableViewSource : ReactiveTableViewSource<AccountItemViewModel>
    {
        public AccountTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<AccountItemViewModel> collection) 
            : base(tableView, collection,  AccountCellView.Key, 74f)
        {
            tableView.RowHeight = 74f;
            tableView.SeparatorInset = new UIEdgeInsets(0, tableView.RowHeight, 0, 0);
            tableView.RegisterClassForCellReuse(typeof(AccountCellView), AccountCellView.Key);
        }

        public override void RowSelected(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            tableView.DeselectRow(indexPath, true);

            var item = ItemAt(indexPath) as AccountItemViewModel;
            if (item != null)
                item.SelectCommand.ExecuteIfCan();
        }

        public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return true;
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return UITableViewCellEditingStyle.Delete;
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
        {
            var vm = ItemAt(indexPath) as AccountItemViewModel;
            if (vm != null)
                vm.DeleteCommand.ExecuteIfCan();
        }
    }
}


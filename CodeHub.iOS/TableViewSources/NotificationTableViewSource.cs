using CodeHub.Core.ViewModels.Activity;
using Foundation;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.Cells;
using System;

namespace CodeHub.iOS.TableViewSources
{
    public class NotificationTableViewSource : ReactiveTableViewSource<NotificationItemViewModel>
    {
        private readonly Func<bool> _canEdit;

        public NotificationTableViewSource(UITableView tableView, IReadOnlyReactiveList<NotificationGroupViewModel> collections, Func<bool> canEdit)
            : base(tableView, UITableView.AutomaticDimension, 64f)
        {
            _canEdit = canEdit;
            tableView.RegisterNibForCellReuse(NotificationTableViewCell.Nib, NotificationTableViewCell.Key);
            Data = collections.CreateDerivedCollection(x => 
                new TableSectionInformation<NotificationItemViewModel, NotificationTableViewCell>(x.Notifications, NotificationTableViewCell.Key, (float)UITableView.AutomaticDimension)
                {
                    Header = new TableSectionHeader(x.Name)
                },
                filter: x => x.IsVisible,
                signalReset: collections.Changed);
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            return "Mark As Read";
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as NotificationItemViewModel;
            if (item == null || !tableView.Editing)
                return;
            item.IsSelected = false;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as NotificationItemViewModel;
            if (item == null)
                return;

            if (tableView.Editing)
                item.IsSelected = true;
            else
            {
                item.GoToCommand.ExecuteIfCan();
            }
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableViewCellEditingStyle.Delete;
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                    var item = ItemAt(indexPath) as NotificationItemViewModel;
                    item?.RemoveCommand.ExecuteIfCan();
                    break;
            }
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return _canEdit();
        }
    }
}


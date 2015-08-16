using System.Collections.Generic;
using System.Linq;
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
        private static readonly nfloat _hintSize = 64f;
        private NotificationViewCell _usedForHeight;

        public NotificationTableViewSource(UITableView tableView)
            : base(tableView, UITableView.AutomaticDimension, _hintSize)
        {
            tableView.RegisterClassForCellReuse(typeof(NotificationViewCell), NotificationViewCell.Key);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (_usedForHeight == null)
                _usedForHeight = NotificationViewCell.Create();

            var item = ItemAt(indexPath) as NotificationItemViewModel;
            if (item == null) 
                return base.GetHeightForRow(tableView, indexPath);

            _usedForHeight.ViewModel = item;
            return _usedForHeight.GetHeight(tableView.Bounds.Size);
        }

        public void SetData(IEnumerable<NotificationGroupViewModel> collections)
        {
            Data = collections.Select(x => 
                new TableSectionInformation<NotificationItemViewModel, NotificationViewCell>(x.Notifications, NotificationViewCell.Key, (float)_hintSize)
                {
                    Header = new TableSectionHeader(x.Name)
                }).ToList();
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

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }
    }
}


using System.Collections.Generic;
using System.Linq;
using CodeHub.Core.ViewModels.Notifications;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.TableViewSources
{
    public class NotificationTableViewSource : ReactiveTableViewSource<NotificationItemViewModel>
    {
        private NotificationViewCell _usedForHeight;

        public NotificationTableViewSource(UITableView tableView)
            : base(tableView)
        {
            tableView.RegisterClassForCellReuse(typeof(NotificationViewCell), NotificationViewCell.Key);
        }

        public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
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
            {
                var headerView = new NotificationHeaderView(x);
                return new TableSectionInformation<NotificationItemViewModel, NotificationViewCell>(x.Notifications,
                    NotificationViewCell.Key, 44f)
                {
                    Header = new TableSectionHeader(() => headerView, 30f)
                };

            }).ToList();
        }

        public override void RowSelected(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as NotificationItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CodeHub.Core.ViewModels.Notifications;
using CodeHub.iOS.Cells;
using CodeHub.iOS.ViewComponents;
using GitHubSharp.Models;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;

namespace CodeHub.iOS.TableViewSources
{
    public class NotificationTableViewSource : ReactiveTableViewSource<NotificationModel>
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

            var item = ItemAt(indexPath) as NotificationModel;
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
                return new TableSectionInformation<NotificationModel, NotificationViewCell>(x.Notifications,
                    NotificationViewCell.Key, 44f)
                {
                    Header = new TableSectionHeader(() => headerView, 30f)
                };

            }).ToList();
        }
    }
}


using System;
using ReactiveUI;
using MonoTouch.UIKit;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Source;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.iOS.TableViewSources
{
    public class CommitedFilesTableViewSource : ReactiveTableViewSource<CommitedFileItemViewModel>
    {
        public CommitedFilesTableViewSource(UITableView tableView)
            : base(tableView)
        {
            tableView.RegisterClassForCellReuse(typeof(CommitedFileTableViewCell), CommitedFileTableViewCell.Key);
        }

        public void SetData(IEnumerable<CommitedFileItemViewModel> collections)
        {
            Data = collections.GroupBy(x => x.RootPath).Select(x =>
            {
                var headerView = new UITableViewHeaderFooterView();
                headerView.TextLabel.Text = x.Key;
                return new TableSectionInformation<CommitedFileItemViewModel, CommitedFileTableViewCell>(
                    new ReactiveList<CommitedFileItemViewModel>(x), CommitedFileTableViewCell.Key, 44f)
                {
                    Header = new TableSectionHeader(() => headerView, 30f)
                };
            }).ToList();
        }

        public override void RowSelected(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as CommitedFileItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}


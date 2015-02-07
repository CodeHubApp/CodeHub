using System;
using ReactiveUI;
using UIKit;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Source;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.iOS.TableViewSources
{
    public class CommitedFilesTableViewSource : ReactiveTableViewSource<CommitedFileItemViewModel>
    {
        public CommitedFilesTableViewSource(UITableView tableView)
            : base(tableView, 44f)
        {
            tableView.RegisterClassForCellReuse(typeof(CommitedFileTableViewCell), CommitedFileTableViewCell.Key);
        }

        public void SetData(IEnumerable<CommitedFileItemViewModel> collections)
        {
            Data = collections
                .GroupBy(x => x.RootPath)
                .Select(x => new TableSectionInformation<CommitedFileItemViewModel, CommitedFileTableViewCell>(
                    new ReactiveList<CommitedFileItemViewModel>(x), CommitedFileTableViewCell.Key, 44f) { Header = new TableSectionHeader(x.Key) })
                .ToList();
        }
    }
}


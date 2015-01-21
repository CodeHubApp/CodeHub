using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Cells;
using System;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class RepositoryTableViewSource : ReactiveTableViewSource<RepositoryItemViewModel>
    {
        private readonly static nfloat _estimatedHeight = 64.0f;

        public RepositoryTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<RepositoryItemViewModel> collection) 
            : base(tableView, collection,  RepositoryCellView.Key, _estimatedHeight)
        {
            tableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
        }

        public RepositoryTableViewSource(UITableView tableView) 
            : base(tableView, _estimatedHeight)
        {
            tableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
        }

        public override nfloat GetHeightForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return UITableView.AutomaticDimension;
        }
    }
}


using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Cells;
using System;
using UIKit;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.TableViewSources
{
    public class RepositoryTableViewSource : ReactiveTableViewSource<RepositoryItemViewModel>
    {
        private readonly static nfloat _estimatedHeight = 64.0f;
        private readonly TableViewCellHeightCache<RepositoryCellView, RepositoryItemViewModel> _cache;
       
        public RepositoryTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<RepositoryItemViewModel> collection) 
            : base(tableView, collection,  RepositoryCellView.Key, UITableView.AutomaticDimension, _estimatedHeight)
        {
            _cache = new TableViewCellHeightCache<RepositoryCellView, RepositoryItemViewModel>(_estimatedHeight, () => RepositoryCellView.Create(true));
            tableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
        }

        public RepositoryTableViewSource(UITableView tableView) 
            : base(tableView, UITableView.AutomaticDimension, _estimatedHeight)
        {
            _cache = new TableViewCellHeightCache<RepositoryCellView, RepositoryItemViewModel>(_estimatedHeight, () => RepositoryCellView.Create(true));
            tableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
        }

        public override nfloat EstimatedHeight(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return _cache[indexPath];
        }

        public override nfloat GetHeightForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as RepositoryItemViewModel;
            var ret = item != null ? _cache.GenerateHeight(tableView, item, indexPath) : base.GetHeightForRow(tableView, indexPath);
            return ret;
        }
    }
}


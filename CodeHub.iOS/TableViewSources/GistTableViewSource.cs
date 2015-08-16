using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Gists;
using System;
using UIKit;
using Foundation;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.TableViewSources
{
    public class GistTableViewSource : ReactiveTableViewSource<GistItemViewModel>
    {
        private readonly TableViewCellHeightCache<GistCellView, GistItemViewModel> _cache;

        public GistTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<GistItemViewModel> collection) 
            : base(tableView, collection, GistCellView.Key, UITableView.AutomaticDimension, 60f)
        {
            _cache = new TableViewCellHeightCache<GistCellView, GistItemViewModel>(60f, () => GistCellView.Create(true));
            tableView.RegisterNibForCellReuse(GistCellView.Nib, GistCellView.Key);
        }

        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return _cache[indexPath];
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as GistItemViewModel;
            return item != null ? _cache.GenerateHeight(tableView, item, indexPath) : base.GetHeightForRow(tableView, indexPath);
        }
    }
}


using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Releases;

namespace CodeHub.iOS.TableViewSources
{
    public class ReleasesTableViewSource : ReactiveTableViewSource<ReleaseItemViewModel>
    {
        public ReleasesTableViewSource(UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<ReleaseItemViewModel> collection) 
            : base(tableView, collection,  ReleaseTableViewCell.Key, 54.0f)
        {
            tableView.RegisterClassForCellReuse(typeof(ReleaseTableViewCell), ReleaseTableViewCell.Key);
        }
    }
}


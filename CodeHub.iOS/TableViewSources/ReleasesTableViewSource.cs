using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Releases;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class ReleasesTableViewSource : ReactiveTableViewSource<ReleaseItemViewModel>
    {
        public ReleasesTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<ReleaseItemViewModel> collection) 
            : base(tableView, collection,  ReleaseTableViewCell.Key, UITableView.AutomaticDimension)
        {
            tableView.RegisterClassForCellReuse(typeof(ReleaseTableViewCell), ReleaseTableViewCell.Key);
        }
    }
}


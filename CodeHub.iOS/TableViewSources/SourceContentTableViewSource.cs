using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Source;

namespace CodeHub.iOS.TableViewSources
{
    public class SourceContentTableViewSource : ReactiveTableViewSource<SourceItemViewModel>
    {
        public SourceContentTableViewSource(UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<SourceItemViewModel> collection) 
            : base(tableView, collection,  SourceContentCellView.Key, 44.0f)
        {
            tableView.RegisterClassForCellReuse(typeof(SourceContentCellView), SourceContentCellView.Key);
        }
    }
}


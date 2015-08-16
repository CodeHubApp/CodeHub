using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Cells;
using Foundation;

namespace CodeHub.iOS.TableViewSources
{
    public class LanguageTableViewSource : ReactiveTableViewSource<LanguageItemViewModel>
    {
        public LanguageTableViewSource(UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<LanguageItemViewModel> collection) 
            : base(tableView, collection, LanguageTableViewCell.Key, 44)
        {
            tableView.RegisterClassForCellReuse(typeof(LanguageTableViewCell), LanguageTableViewCell.Key);
        }
    }
}


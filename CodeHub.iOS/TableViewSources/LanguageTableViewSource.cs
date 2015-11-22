using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Cells;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class LanguageTableViewSource : ReactiveTableViewSource<LanguageItemViewModel>
    {
        public LanguageTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<LanguageItemViewModel> collection) 
            : base(tableView, collection, LanguageTableViewCell.Key, UITableView.AutomaticDimension)
        {
            tableView.RegisterClassForCellReuse(typeof(LanguageTableViewCell), LanguageTableViewCell.Key);
        }
    }
}


using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Cells;
using MonoTouch.Foundation;

namespace CodeHub.iOS.TableViewSources
{
    public class LanguageTableViewSource : ReactiveTableViewSource<LanguageItemViewModel>
    {
        public LanguageTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<LanguageItemViewModel> collection) 
            : base(tableView, collection, LanguageTableViewCell.Key, 44f)
        {
            tableView.RegisterClassForCellReuse(typeof(LanguageTableViewCell), LanguageTableViewCell.Key);
        }
    }
}


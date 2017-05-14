using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.TableViewCells;
using System;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class RepositoryTableViewSource : ReactiveTableViewSource<RepositoryItemViewModel>
    {
        private readonly static nfloat _estimatedHeight = 100.0f;

        public RepositoryTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<RepositoryItemViewModel> collection)
            : base(tableView, collection, RepositoryCellView.Key, UITableView.AutomaticDimension, _estimatedHeight)
        {
            tableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
        }

        public RepositoryTableViewSource(UITableView tableView)
            : base(tableView, UITableView.AutomaticDimension, _estimatedHeight)
        {
            tableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
        }
    }
}


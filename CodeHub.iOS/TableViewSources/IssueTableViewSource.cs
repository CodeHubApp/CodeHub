using ReactiveUI;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Cells;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CodeHub.iOS.TableViewSources
{
    public class IssueTableViewSource : ReactiveTableViewSource<IssueItemViewModel>
    {
        private const float HintSize = 69.0f;

        public IssueTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<IssueItemViewModel> collection) 
            : base(tableView, collection, IssueCellView.Key, HintSize)
        {
            tableView.RegisterNibForCellReuse(IssueCellView.Nib, IssueCellView.Key);
        }

        public IssueTableViewSource(MonoTouch.UIKit.UITableView tableView) 
            : base(tableView, HintSize)
        {
            tableView.RegisterNibForCellReuse(IssueCellView.Nib, IssueCellView.Key);
        }

        public void SetData(IEnumerable<IssueGroupViewModel> collections)
        {
            Data = collections.Select(x =>
            {
                return new TableSectionInformation<IssueItemViewModel, IssueCellView>(x.Issues, IssueCellView.Key, HintSize)
                {
                    Header = new TableSectionHeader(x.Name)
                };
            }).ToList();
        }

        public override void RowSelected(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as IssueItemViewModel;
            if (item != null)
            {
                if (!item.GoToCommand.CanExecute(null))
                    Console.WriteLine("Cant execute!");
                item.GoToCommand.ExecuteIfCan();
            }
            tableView.DeselectRow(indexPath, true);
        }
    }
}


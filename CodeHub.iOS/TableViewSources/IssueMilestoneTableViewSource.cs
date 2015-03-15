using ReactiveUI;
using CodeHub.iOS.Cells;
using UIKit;
using CodeHub.Core.ViewModels.Issues;
using System;

namespace CodeHub.iOS.TableViewSources
{
    public class IssueMilestoneTableViewSource : ReactiveTableViewSource<IssueMilestoneItemViewModel>
    {
        public IssueMilestoneTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<IssueMilestoneItemViewModel> collection) 
            : base(tableView, collection,  MilestoneTableViewCell.Key, 80f)
        {
            tableView.RegisterClassForCellReuse(typeof(MilestoneTableViewCell), MilestoneTableViewCell.Key);

            collection.CountChanged.Subscribe(x => {
                Console.WriteLine("Mielstones shoudlb e: " + x);
            });
        }

        public override void RowSelected(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            tableView.DeselectRow(indexPath, true);
        }
    }
}


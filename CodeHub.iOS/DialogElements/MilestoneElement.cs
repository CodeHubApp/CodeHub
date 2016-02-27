using System;
using Foundation;
using UIKit;
using CodeHub.iOS.TableViewCells;

namespace CodeHub.iOS.DialogElements
{
    public class MilestoneElement : Element
    {
        private readonly string _title;
        private readonly int _openIssues;
        private readonly int _closedIssues;
        private readonly DateTimeOffset? _dueDate;

        public event Action Tapped;

        public UITableViewCellAccessory Accessory = UITableViewCellAccessory.None;

        public int Number { get; private set; }

        public MilestoneElement(int number, string title, int openIssues, int closedIssues, DateTimeOffset? dueDate)
        {
            Number = number;
            _title = title;
            _openIssues = openIssues;
            _closedIssues = closedIssues;
            _dueDate = dueDate;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell(MilestoneTableViewCell.Key) as MilestoneTableViewCell ?? new MilestoneTableViewCell
            {
                SelectionStyle = UITableViewCellSelectionStyle.Blue
            };

            cell.Accessory = Accessory;
            cell.Init(_title, _openIssues, _closedIssues, _dueDate);
            return cell;
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            Tapped?.Invoke();
        }
    }
}


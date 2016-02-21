using System;
using UIKit;
using Foundation;
using CodeHub.iOS.TableViewCells;

namespace CodeHub.iOS.DialogElements
{
    public class IssueElement : Element, IElementSizing
    {       
        public UITableViewCellStyle Style { get; set; }
        public UIColor BackgroundColor { get; set; }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Assigned { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Kind { get; set; }
        public DateTimeOffset LastUpdated { get; set; }

        public IssueElement(string id, string title, string assigned, string status, string priority, string kind, DateTimeOffset lastUpdated) 
        {
            if (string.IsNullOrEmpty(assigned))
                assigned = "unassigned";

            Id = id;
            Title = title;
            Assigned = assigned;
            Status = status;
            Priority = priority;
            Kind = kind;
            LastUpdated = lastUpdated;
            Style = UITableViewCellStyle.Default;
        }

        public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            return 69f;
        }

        public event Action Tapped;

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell(IssueCellView.Key) as IssueCellView ?? IssueCellView.Create();
            cell.Bind(Title, Status, Priority, Assigned, LastUpdated, Id, Kind);
            return cell;
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            Tapped?.Invoke();
        }

        public override bool Matches(string text)
        {
            var id = Id ?? string.Empty;
            var title = Title ?? string.Empty;

            return id.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1 || title.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}


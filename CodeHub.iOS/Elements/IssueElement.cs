using System;
using CodeFramework.iOS.Cells;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Xamarin.Utilities.DialogElements;

namespace CodeFramework.Elements
{
    public class IssueElement : Element, IElementSizing, IColorizeBackground
    {       
        public UITableViewCellStyle Style { get; set; }
        public UIColor BackgroundColor { get; set; }
        public object Tag { get; set; }

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

        public float GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            return 69f;
        }

        public event NSAction Tapped;

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell(IssueCellView.Key) as IssueCellView ?? IssueCellView.Create();
            cell.Bind(Title, Status, Priority, Assigned, LastUpdated.ToDaysAgo(), Id, Kind);
            return cell;
        }



        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            if (Tapped != null)
                Tapped();
        }

        void IColorizeBackground.WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
        }

        public override bool Matches(string text)
        {
            var lowerText = text.ToLower();
            return Id.ToLower().Equals(lowerText) || Title.ToLower().Contains(lowerText);
        }
    }
}


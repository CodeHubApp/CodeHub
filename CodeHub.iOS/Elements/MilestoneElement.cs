using System;
using System.Drawing;
using CodeHub.iOS.ViewComponents;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Elements
{
    public class MilestoneElement : Element, IElementSizing
    {
        public MilestoneModel Milestone { get; private set; }
        public Action Tapped;

        public UITableViewCellAccessory Accessory = UITableViewCellAccessory.None;

        public MilestoneElement(MilestoneModel m)
        {
            Milestone = m;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell(MilestoneTableViewCell.Key) as MilestoneTableViewCell ?? new MilestoneTableViewCell
            {
                SelectionStyle = UITableViewCellSelectionStyle.Blue
            };

            cell.Accessory = Accessory;
            cell.Init(Milestone.Title, Milestone.OpenIssues, Milestone.ClosedIssues, Milestone.DueOn);
            return cell;
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            if (Tapped != null)
                Tapped();
            base.Selected(tableView, path);
        }

        public float GetHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return 80f;
        }

        private class MilestoneTableViewCell : UITableViewCell
        {
            public const string Key = "milestonecellview";
            private readonly MilestoneView _milestoneView;

            public void Init(string title, int openIssues, int closedIssues, DateTimeOffset? dueDate)
            {
                _milestoneView.Init(title, openIssues, closedIssues, dueDate);
            }

            public override void SetSelected(bool selected, bool animated)
            {
                BackgroundColor = selected ? UIColor.FromWhiteAlpha(0.9f, 1.0f) : UIColor.White;
            }

            public override void SetHighlighted(bool highlighted, bool animated)
            {
                BackgroundColor = highlighted ? UIColor.FromWhiteAlpha(0.9f, 1.0f) : UIColor.White;
            }

            public MilestoneTableViewCell()
                : base(UITableViewCellStyle.Default, Key)
            {
                Frame = new RectangleF(0, 0, 320f, 80);
                AutosizesSubviews = true;
                ContentView.AutosizesSubviews = true;
                SeparatorInset = UIEdgeInsets.Zero;

                _milestoneView = new MilestoneView();
                _milestoneView.Frame = this.Frame;
                _milestoneView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                ContentView.Add(_milestoneView);
            }
        }
    }
}
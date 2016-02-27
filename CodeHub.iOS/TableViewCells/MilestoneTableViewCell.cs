using System;
using UIKit;
using CodeHub.iOS.Views;
using CoreGraphics;
using Foundation;

namespace CodeHub.iOS.TableViewCells
{
    public class MilestoneTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("MilestoneTableViewCell");
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

        public override NSString ReuseIdentifier
        {
            get
            {
                return Key;
            }
        }

        public MilestoneTableViewCell()
            : base(new CGRect(0, 0, 320f, 80))
        {
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


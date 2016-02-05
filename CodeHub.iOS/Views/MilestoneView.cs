using System;
using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.Views
{
    public class MilestoneView : UIView
    {
        private readonly UILabel _titleLabel;
        private readonly UILabel _openClosedLabel;
        private readonly UILabel _dueLabel;
        private readonly ProgressBarView _progressView;

        public void Init(string title, int openIssues, int closedIssues, DateTimeOffset? dueDate)
        {
            _titleLabel.Text = title;
            _openClosedLabel.Text = string.Format("{0} closed - {1} open", closedIssues, openIssues);

            var totalIssues = closedIssues + openIssues;
            var percentage = totalIssues == 0 ? 0 : ((float)closedIssues / totalIssues);
            _progressView.Percentage = (int)Math.Ceiling(percentage * 100);
            if (dueDate.HasValue)
            {
                var remainingDays = (int)Math.Ceiling((dueDate.Value - DateTimeOffset.Now).TotalDays);
                if (remainingDays == 0)
                    _dueLabel.Text = "Due Today";
                else if (remainingDays > 0)
                    _dueLabel.Text = "Due in " + remainingDays + " days";
                else if (remainingDays < 0)
                    _dueLabel.Text = "Overdue " + Math.Abs(remainingDays) + " days";
            }
            else
                _dueLabel.Text = "No Due Date";
        }

        public MilestoneView()
            : base(new CGRect(0, 0, 320f, 80))
        {
            AutosizesSubviews = true;

            _titleLabel = new UILabel();
            _titleLabel.Frame = new CGRect(10f, 10, Frame.Width - 20f, 18f);
            _titleLabel.Font = UIFont.BoldSystemFontOfSize(16f);
            _titleLabel.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            Add(_titleLabel);

            _openClosedLabel = new UILabel();
            _openClosedLabel.Frame = new CGRect(10f, _titleLabel.Frame.Bottom + 1f, 150f, 12f);
            _openClosedLabel.AutoresizingMask = UIViewAutoresizing.FlexibleRightMargin;
            _openClosedLabel.Font = UIFont.SystemFontOfSize(11f);
            Add(_openClosedLabel);

            _dueLabel = new UILabel();
            _dueLabel.Frame = new CGRect(Frame.Width - 150f, _titleLabel.Frame.Bottom + 1f, 150f - 10f, 12f);
            _dueLabel.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin;
            _dueLabel.TextAlignment = UITextAlignment.Right;
            _dueLabel.TextColor = UIColor.DarkGray;
            _dueLabel.Font = UIFont.SystemFontOfSize(11f);
            Add(_dueLabel);

            _progressView = new ProgressBarView();
            _progressView.Frame = new CGRect(10f, _openClosedLabel.Frame.Bottom + 9f, Frame.Width - 20f, 20f);
            _progressView.Layer.MasksToBounds = true;
            _progressView.Layer.CornerRadius = 4f;
            _progressView.BackgroundColor = UIColor.Gray;
            _progressView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            Add(_progressView);
        }
    }
}


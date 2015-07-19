using System;
using CoreGraphics;
using UIKit;

namespace CodeHub.iOS.Views
{
    public class MilestoneView : UIView
    {
        private readonly UILabel _titleLabel;
        private readonly UILabel _openClosedLabel;
        private readonly UILabel _dueLabel;
        private readonly ProgressBarView _progressView;

        public string Title
        {
            get { return _titleLabel.Text; }
            set { _titleLabel.Text = value; }
        }

        public DateTimeOffset? DueDate
        {
            set
            {
                if (value.HasValue)
                {
                    var remainingDays = (int)Math.Ceiling((value.Value - DateTimeOffset.Now).TotalDays);
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
        }

        public Tuple<int, int> OpenClosedIssues
        {
            set
            {
                _openClosedLabel.Text = string.Format("{0} closed - {1} open", value.Item2, value.Item1);
                var totalIssues = value.Item2 + value.Item1;
                var percentage = totalIssues == 0 ? 0 : ((float)value.Item2 / totalIssues);
                _progressView.Percentage = (int)Math.Ceiling(percentage * 100);
            }
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
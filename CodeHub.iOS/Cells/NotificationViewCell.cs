using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Notifications;

namespace CodeHub.iOS.Cells
{
    public class NotificationViewCell : ReactiveTableViewCell<NotificationItemViewModel>
    {
        public static NSString Key = new NSString("NotificationTableViewCell");

        [Export("initWithStyle:reuseIdentifier:")]
        public NotificationViewCell(UITableViewCellStyle style, NSString reuseIdentifier)
            : base(UITableViewCellStyle.Subtitle, reuseIdentifier)
        {
            TextLabel.Lines = 0;
            TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
            TextLabel.Font = UIFont.BoldSystemFontOfSize(14f);
            TextLabel.TextColor = Theme.MainTitleColor;

            DetailTextLabel.Font = UIFont.SystemFontOfSize(12f);
            DetailTextLabel.TextColor = Theme.MainTextColor;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    TextLabel.Text = x.Title;
                    DetailTextLabel.Text = x.UpdatedAt;

                    switch (x.Type)
                    {
                        case NotificationItemViewModel.NotificationType.Issue:
                            ImageView.Image = Images.Flag;
                            break;
                        case NotificationItemViewModel.NotificationType.PullRequest:
                            ImageView.Image = Images.Hand;
                            break;
                        case NotificationItemViewModel.NotificationType.Commit:
                            ImageView.Image = Images.Commit;
                            break;
                        case NotificationItemViewModel.NotificationType.Release:
                            ImageView.Image = Images.Tag;
                            break;
                        default:
                            ImageView.Image = Images.Notifications;
                            break;
                    }
                });
        }

        public static NotificationViewCell Create()
        {
            return new NotificationViewCell(UITableViewCellStyle.Default, Key);
        }

        public float GetHeight(SizeF bounds)
        {
            Bounds = new RectangleF(new PointF(0, 0), bounds);
            SetNeedsLayout();
            LayoutIfNeeded();

            var textHeight = GetLabelHeight(TextLabel);
            var detailHeight = GetLabelHeight(DetailTextLabel);
            var total = textHeight + detailHeight + 20f;
            return Math.Max(total, 44f);
        }

        private static float GetLabelHeight(UILabel label)
        {
            var textString = new NSString(label.Text);
            return textString.StringSize(label.Font, new SizeF(label.Bounds.Width, float.MaxValue), label.LineBreakMode).Height;
        }
    }
}


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
            DetailTextLabel.TextColor = Theme.MainSubtitleColor;
            ImageView.TintColor = Themes.Theme.Current.PrimaryNavigationBarColor;

            var issueOpened = Images.IssueOpened.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            var pullRequest = Images.PullRequest.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            var commit = Images.Commit.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            var tag = Images.Tag.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            var inbox = Images.Inbox.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    TextLabel.Text = x.Title;
                    DetailTextLabel.Text = x.UpdatedAt;

                    switch (x.Type)
                    {
                        case NotificationItemViewModel.NotificationType.Issue:
                            ImageView.Image = issueOpened;
                            break;
                        case NotificationItemViewModel.NotificationType.PullRequest:
                            ImageView.Image = pullRequest;
                            break;
                        case NotificationItemViewModel.NotificationType.Commit:
                            ImageView.Image = commit;
                            break;
                        case NotificationItemViewModel.NotificationType.Release:
                            ImageView.Image = tag;
                            break;
                        default:
                            ImageView.Image = inbox;
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


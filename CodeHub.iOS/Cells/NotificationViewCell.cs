using System;
using CoreGraphics;
using Foundation;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Activity;

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

            this.OneWayBind(ViewModel, x => x.Title, x => x.TextLabel.Text);
            this.OneWayBind(ViewModel, x => x.UpdatedAt, x => x.DetailTextLabel.Text);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    switch (x.Type)
                    {
                        case NotificationItemViewModel.NotificationType.Issue:
                            ImageView.Image = Octicon.IssueOpened.ToImage(ImageView.Frame.Width);
                            break;
                        case NotificationItemViewModel.NotificationType.PullRequest:
                            ImageView.Image = Octicon.GitPullRequest.ToImage(ImageView.Frame.Width);
                            break;
                        case NotificationItemViewModel.NotificationType.Commit:
                            ImageView.Image = Octicon.GitCommit.ToImage(ImageView.Frame.Width);
                            break;
                        case NotificationItemViewModel.NotificationType.Release:
                            ImageView.Image = Octicon.Tag.ToImage(ImageView.Frame.Width);
                            break;
                        default:
                            ImageView.Image = Octicon.Inbox.ToImage(ImageView.Frame.Width);
                            break;
                    }
                });
        }

        public static NotificationViewCell Create()
        {
            return new NotificationViewCell(UITableViewCellStyle.Default, Key);
        }

        public nfloat GetHeight(CGSize bounds)
        {
            Bounds = new CGRect(new CGPoint(0, 0), bounds);
            SetNeedsLayout();
            LayoutIfNeeded();

            var textHeight = GetLabelHeight(TextLabel);
            var detailHeight = GetLabelHeight(DetailTextLabel);
            var total = textHeight + detailHeight + 20f;
            return (nfloat)Math.Max(total, 44f);
        }

        private static nfloat GetLabelHeight(UILabel label)
        {
            var textString = new NSString(label.Text);
            return textString.StringSize(label.Font, new CGSize(label.Bounds.Width, float.MaxValue), label.LineBreakMode).Height;
        }
    }
}


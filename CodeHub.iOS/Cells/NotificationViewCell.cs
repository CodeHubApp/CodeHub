using System;
using System.Drawing;
using GitHubSharp.Models;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using Humanizer;

namespace CodeHub.iOS.Cells
{
    public class NotificationViewCell : ReactiveTableViewCell<NotificationModel>
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
                    TextLabel.Text = x.Subject.Title;
                    DetailTextLabel.Text = x.UpdatedAt.UtcDateTime.Humanize();

                    var subject = x.Subject.Type.ToLower();
                    if (subject.Equals("issue"))
                        ImageView.Image = Images.Flag;
                    else if (subject.Equals("pullrequest"))
                        ImageView.Image = Images.Hand;
                    else if (subject.Equals("commit"))
                        ImageView.Image = Images.Commit;
                    else if (subject.Equals("release"))
                        ImageView.Image = Images.Tag;
                    else
                        ImageView.Image = Images.Notifications;
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


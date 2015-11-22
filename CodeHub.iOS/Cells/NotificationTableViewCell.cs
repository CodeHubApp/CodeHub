using System;
using Foundation;
using UIKit;
using CodeHub.Core.ViewModels.Activity;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.Cells
{
    public partial class NotificationTableViewCell : ReactiveTableViewCell<NotificationItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("NotificationTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("NotificationTableViewCell");

        public NotificationTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            DescriptionLabel.TextColor = Theme.MainTitleColor;
            TimeLabel.TextColor = Theme.MainSubtitleColor;

            this.OneWayBind(ViewModel, x => x.Title, x => x.DescriptionLabel.Text);
            this.OneWayBind(ViewModel, x => x.UpdatedAt, x => x.TimeLabel.Text);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                    {
                        switch (x.Type)
                        {
                            case NotificationItemViewModel.NotificationType.Issue:
                                IconImageView.Image = Octicon.IssueOpened.ToImage();
                                break;
                            case NotificationItemViewModel.NotificationType.PullRequest:
                                IconImageView.Image = Octicon.GitPullRequest.ToImage();
                                break;
                            case NotificationItemViewModel.NotificationType.Commit:
                                IconImageView.Image = Octicon.GitCommit.ToImage();
                                break;
                            case NotificationItemViewModel.NotificationType.Release:
                                IconImageView.Image = Octicon.Tag.ToImage();
                                break;
                            default:
                                IconImageView.Image = Octicon.Inbox.ToImage();
                                break;
                        }
                    });
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseDesignerOutlets();
            base.Dispose(disposing);
        }
    }
}


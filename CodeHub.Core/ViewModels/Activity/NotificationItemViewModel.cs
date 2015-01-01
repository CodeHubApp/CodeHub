using System;
using ReactiveUI;
using Humanizer;
using Octokit;

namespace CodeHub.Core.ViewModels.Notifications
{
    public class NotificationItemViewModel : ReactiveObject
    {
        public string Title { get; private set; }

        public string UpdatedAt { get; private set; }

        public NotificationType Type { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        public string Id { get; private set; }

        internal NotificationItemViewModel(Notification notification, Action<Notification> gotoAction)
        {
            Title = notification.Subject.Title;
            Id = notification.Id;
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoAction(notification));

            var subject = notification.Subject.Type.ToLower();
            if (subject.Equals("issue"))
                Type = NotificationType.Issue;
            else if (subject.Equals("pullrequest"))
                Type = NotificationType.PullRequest;
            else if (subject.Equals("commit"))
                Type = NotificationType.Commit;
            else if (subject.Equals("release"))
                Type = NotificationType.Release;
            else
                Type = NotificationType.Unknown;

            try
            {
                UpdatedAt = DateTimeOffset.Parse(notification.UpdatedAt).LocalDateTime.Humanize();
            }
            catch
            {
                // Back up plan?
                UpdatedAt = DateTimeOffset.Now.LocalDateTime.Humanize();
            }
        }

        public enum NotificationType
        {
            Unknown,
            Issue,
            PullRequest,
            Commit,
            Release
        }
    }
}


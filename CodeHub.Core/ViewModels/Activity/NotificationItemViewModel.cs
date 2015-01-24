using System;
using ReactiveUI;
using Humanizer;
using Octokit;

namespace CodeHub.Core.ViewModels.Activity
{
    public class NotificationItemViewModel : ReactiveObject
    {
        public string Title { get; private set; }

        public string UpdatedAt { get; private set; }

        public NotificationType Type { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        public string Id { get; private set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        internal Notification Notification { get; private set; }

        internal NotificationItemViewModel(Notification notification, Action<NotificationItemViewModel> gotoAction)
        {
            Notification = notification;
            Title = notification.Subject.Title;
            Id = notification.Id;

            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoAction(this));

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


using System;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Notifications
{
    public class NotificationGroupViewModel : ReactiveObject
    {
        public IReactiveCommand ReadAllCommand { get; private set; }

        public string Name { get; private set; }

        public IReadOnlyReactiveList<NotificationModel> Notifications { get; private set; }

        public NotificationGroupViewModel(string name, IReadOnlyReactiveList<NotificationModel> notifications)
        {
            Notifications = notifications;
            Name = name;
        }

        public NotificationGroupViewModel(string name, IReadOnlyReactiveList<NotificationModel> notifications, Action<NotificationGroupViewModel> readAll)
            : this(name, notifications)
        {
            if (readAll != null)
                ReadAllCommand = ReactiveCommand.Create().WithSubscription(_ => readAll(this));
        }
    }
}
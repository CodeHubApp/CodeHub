using System;
using ReactiveUI;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Notifications
{
    public class NotificationGroupViewModel : ReactiveObject
    {
        public IReactiveCommand ReadAllCommand { get; private set; }

        public string Name { get; private set; }

        public IReadOnlyReactiveList<NotificationItemViewModel> Notifications { get; private set; }

        public NotificationGroupViewModel(string name, IReadOnlyReactiveList<NotificationItemViewModel> notifications)
        {
            Notifications = notifications;
            Name = name;
        }

        public NotificationGroupViewModel(string name, IReadOnlyReactiveList<NotificationItemViewModel> notifications, Func<NotificationGroupViewModel, Task> readAll)
            : this(name, notifications)
        {
            if (readAll != null)
                ReadAllCommand = ReactiveCommand.CreateAsyncTask(_ => readAll(this));
        }
    }
}
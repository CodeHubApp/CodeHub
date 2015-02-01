using ReactiveUI;

namespace CodeHub.Core.ViewModels.Activity
{
    public class NotificationGroupViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public IReadOnlyReactiveList<NotificationItemViewModel> Notifications { get; private set; }

        public NotificationGroupViewModel(string name, IReadOnlyReactiveList<NotificationItemViewModel> notifications)
        {
            Notifications = notifications;
            Name = name;
        }
    }
}
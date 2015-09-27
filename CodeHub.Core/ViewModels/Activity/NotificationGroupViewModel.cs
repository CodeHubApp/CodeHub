using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Activity
{
    public class NotificationGroupViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public IReadOnlyReactiveList<NotificationItemViewModel> Notifications { get; private set; }

        private readonly ObservableAsPropertyHelper<bool> _visible;
        public bool IsVisible
        {
            get { return _visible.Value; }
        }

        public NotificationGroupViewModel(string name, IReadOnlyReactiveList<NotificationItemViewModel> notifications)
        {
            Notifications = notifications;
            Name = name;

            _visible = notifications.CountChanged
                .StartWith(notifications.Count)
                .Select(x => x > 0)
                .ToProperty(this, x => x.IsVisible);
        }
    }
}
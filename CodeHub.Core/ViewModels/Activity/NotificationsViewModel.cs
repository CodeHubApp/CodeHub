using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.ViewModels.Source;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System.Reactive.Subjects;

namespace CodeHub.Core.ViewModels.Activity
{
    public class NotificationsViewModel : BaseViewModel, ILoadableViewModel
    {
        public const int UnreadFilter = 0;
        public const int ParticipatingFilter = 1;
        public const int AllFilter = 2;

        private readonly ReactiveList<Octokit.Notification> _notifications = new ReactiveList<Octokit.Notification>();
        private readonly ISubject<int> _notificationCount = new Subject<int>();
        private readonly ISessionService _applicationService;

        public IReadOnlyReactiveList<NotificationItemViewModel> Notifications { get; }

        public IReadOnlyReactiveList<NotificationGroupViewModel> GroupedNotifications { get; }

        private int _activeFilter;
        public int ActiveFilter
		{
			get { return _activeFilter; }
			set { this.RaiseAndSetIfChanged(ref _activeFilter, value); }
		}

        public IObservable<int> NotificationCount
        {
            get { return _notificationCount.AsObservable(); }
        }

        private readonly ObservableAsPropertyHelper<bool> _showEditButton;
        public bool ShowEditButton
        {
            get { return _showEditButton.Value; }
        }

        private readonly ObservableAsPropertyHelper<bool> _anyItemsSelected;
        public bool IsAnyItemsSelected
        {
            get { return _anyItemsSelected.Value; }
        }


        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> ReadSelectedCommand { get; private set; }

        public NotificationsViewModel(ISessionService applicationService)
        {
            _applicationService = applicationService;
            Title = "Notifications";

            _showEditButton = this.WhenAnyValue(x => x.ActiveFilter)
                .Select(x => x != AllFilter)
                .ToProperty(this, x => x.ShowEditButton);

            var groupedNotifications = new ReactiveList<NotificationGroupViewModel>();
            GroupedNotifications = groupedNotifications;

            Notifications = _notifications.CreateDerivedCollection(y => new NotificationItemViewModel(y, GoToNotification, DeleteNotification));
            Notifications.Changed.Where(x => x.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                .Select(_ => Notifications)
                .Subscribe(notifications => groupedNotifications.Reset(notifications.GroupBy(x => x.Notification.Repository.FullName).Select(x => {
                var items = notifications.CreateDerivedCollection(y => y, filter: y => y.Notification.Repository.FullName == x.Key);
                return new NotificationGroupViewModel(x.Key, items);
            })));

            _anyItemsSelected = Notifications.Changed
                .SelectMany(x => Notifications)
                .Select(x => x.WhenAnyValue(y => y.IsSelected))
                .Merge()
                .Select(x => Notifications.Select(y => y.IsSelected).Any(y => y))
                .ToProperty(this, x => x.IsAnyItemsSelected);
  
            ReadSelectedCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                if (GroupedNotifications.SelectMany(x => x.Notifications).All(x => !x.IsSelected))
                {
                    var request = new Octokit.MarkAsReadRequest { LastReadAt = DateTimeOffset.Now };
                    applicationService.GitHubClient.Notification.MarkAsRead(request).ToBackground();
                    _notifications.Clear();
                }
                else
                {
                    var selected = GroupedNotifications.SelectMany(x => x.Notifications)
                        .Where(x => x.IsSelected && x.Notification.Unread).ToList();

                    var tasks = selected
                        .Select(t =>  _applicationService.GitHubClient.Notification.MarkAsRead(int.Parse(t.Id)));

                    Task.WhenAll(tasks).ToBackground();

                    _notifications.RemoveAll(selected.Select(y => y.Notification));
                }
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                var all = ActiveFilter == AllFilter;
                var participating = ActiveFilter == ParticipatingFilter;
                var req = new Octokit.NotificationsRequest { All = all, Participating = participating, Since = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(365)) };
                _notifications.Reset(await applicationService.GitHubClient.Notification.GetAllForCurrent(req));
            });

            _notifications.CountChanged
                .Where(_ => ActiveFilter == UnreadFilter)
                .Subscribe(_notificationCount.OnNext);

            this.WhenAnyValue(x => x.ActiveFilter).Skip(1).Subscribe(x =>
            {
                _notifications.Clear();
                LoadCommand.ExecuteIfCan();
            });
        }

        private async Task ReadNotification(Octokit.Notification notification)
        {
            if (notification == null || !notification.Unread) return;
            await _applicationService.GitHubClient.Notification.MarkAsRead(int.Parse(notification.Id));
            _notifications.Remove(notification);
        }

        private void DeleteNotification(NotificationItemViewModel item)
        {
            _notifications.Remove(item.Notification);
            ReadNotification(item.Notification).ToBackground();
        }

        private void GoToNotification(NotificationItemViewModel item)
        {
            var x = item.Notification;
            var subject = x.Subject.Type.ToLower();
            if (subject.Equals("issue"))
            {
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                var vm = this.CreateViewModel<IssueViewModel>();
                vm.Init(x.Repository.Owner.Login, x.Repository.Name, int.Parse(node)); 
                NavigateTo(vm);
            }
            else if (subject.Equals("pullrequest"))
            {
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                var vm = this.CreateViewModel<PullRequestViewModel>();
                vm.Init(x.Repository.Owner.Login, x.Repository.Name, int.Parse(node)); 
                NavigateTo(vm);
            }
            else if (subject.Equals("commit"))
            {
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                var vm = this.CreateViewModel<CommitViewModel>();
                vm.Init(x.Repository.Owner.Login, x.Repository.Name, node);
                NavigateTo(vm);
            }
            else if (subject.Equals("release"))
            {
                var vm = this.CreateViewModel<BranchesAndTagsViewModel>();
                vm.RepositoryOwner = x.Repository.Owner.Login;
                vm.RepositoryName = x.Repository.Name;
                vm.SelectedFilter = BranchesAndTagsViewModel.ShowIndex.Tags;
                NavigateTo(vm);
            }

            ReadNotification(x).ToBackground();
        }
    }
}


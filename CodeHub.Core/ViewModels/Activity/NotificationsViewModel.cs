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
        private const int UnreadFilter = 0;
        private const int ParticipatingFilter = 1;
        private const int AllFilter = 2;

        private readonly ReactiveList<Octokit.Notification> _notifications = new ReactiveList<Octokit.Notification>();
        private readonly ISubject<int> _notificationCount = new Subject<int>();
        private readonly ISessionService _applicationService;

        private IList<NotificationGroupViewModel> _groupedNotifications;
        public IList<NotificationGroupViewModel> GroupedNotifications
        {
            get { return _groupedNotifications; }
            private set { this.RaiseAndSetIfChanged(ref _groupedNotifications, value); }
        }

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

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> ReadSelectedCommand { get; private set; }

        public NotificationsViewModel(ISessionService applicationService)
        {
            _applicationService = applicationService;
            Title = "Notifications";
  
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

            _notifications.Changed.Select(_ => Unit.Default)
                .Merge(_notifications.ItemChanged.Select(_ => Unit.Default))
                .Subscribe(_ =>
                {
                    GroupedNotifications = _notifications.GroupBy(x => x.Repository.FullName).Select(x => 
                    {
                        var items = x.Select(y => new NotificationItemViewModel(y, GoToNotification));
                        var notifications = new ReactiveList<NotificationItemViewModel>(items);
                        return new NotificationGroupViewModel(x.Key, notifications);
                    }).ToList();
                });


            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                var all = ActiveFilter == AllFilter;
                var participating = ActiveFilter == ParticipatingFilter;
                var req = new Octokit.NotificationsRequest { All = all, Participating = participating, Since = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(365)) };
                var notifications = await applicationService.GitHubClient.Notification.GetAllForCurrent(req);
                _notifications.Reset(notifications);
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


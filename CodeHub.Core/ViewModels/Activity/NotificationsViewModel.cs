using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.ViewModels.Source;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Notifications
{
    public class NotificationsViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly ReactiveList<Octokit.Notification> _notifications = new ReactiveList<Octokit.Notification>(); 
        private readonly IApplicationService _applicationService;

        private IList<NotificationGroupViewModel> _groupedNotifications;
        public IList<NotificationGroupViewModel> GroupedNotifications
        {
            get { return _groupedNotifications; }
            private set { this.RaiseAndSetIfChanged(ref _groupedNotifications, value); }
        }

        private int _shownIndex;
		public int ShownIndex
		{
			get { return _shownIndex; }
			set { this.RaiseAndSetIfChanged(ref _shownIndex, value); }
		}

        private NotificationsFilterModel _filter;
        public NotificationsFilterModel Filter
        {
            get { return _filter; }
            private set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand ReadRepositoriesCommand { get; private set; }

        public IReactiveCommand ReadAllCommand { get; private set; }

        public NotificationsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Title = "Notifications";

            var canReadAll = _notifications.CountChanged.Select(x => x > 0).CombineLatest(
                this.WhenAnyValue(x => x.ShownIndex).Select(x => x != 2), (x, y) => x & y);

            ReadAllCommand = ReactiveCommand.CreateAsyncTask(canReadAll, async t =>
            {
                await applicationService.Client.ExecuteAsync(applicationService.Client.Notifications.MarkAsRead());
                _notifications.Clear();
            });

            var readRepositories = new Func<string, Task>(async repo =>
            {
                if (repo == null) return;
                var repoId = new RepositoryIdentifier(repo);
                await applicationService.GitHubClient.Notification.MarkAsReadForRepository(repoId.Owner, repoId.Name);
                _notifications.RemoveAll(_notifications.Where(x => string.Equals(x.Repository.FullName, repo, StringComparison.OrdinalIgnoreCase)).ToList());
            });

            _notifications.Changed.Select(_ => Unit.Default)
                .Merge(_notifications.ItemChanged.Select(_ => Unit.Default))
                .Subscribe(_ =>
                {
                    GroupedNotifications = _notifications.GroupBy(x => x.Repository.FullName).Select(x => 
                    {
                        var items = x.Select(y => new NotificationItemViewModel(y, GoToNotification));
                        var notifications = new ReactiveList<NotificationItemViewModel>(items);
                        return new NotificationGroupViewModel(x.Key, notifications, y => readRepositories(y.Name));
                    }).ToList();
                });


            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var req = new Octokit.NotificationsRequest { All = Filter.All, Participating = Filter.Participating, Since = DateTimeOffset.MinValue };
                var notifictions = await applicationService.GitHubClient.Notification.GetAllForCurrent(req);
                _notifications.Reset(notifictions);
            });

            this.WhenAnyValue(x => x.ShownIndex).Subscribe(x =>
            {
                switch (x)
                {
                    case 0:
                        Filter = NotificationsFilterModel.CreateUnreadFilter();
                        break;
                    case 1:
                        Filter = NotificationsFilterModel.CreateParticipatingFilter();
                        break;
                    default:
                        Filter = NotificationsFilterModel.CreateAllFilter();
                        break;
                }
            });

            this.WhenAnyValue(x => x.Filter).Skip(1).Subscribe(x => {
                _notifications.Clear();
                LoadCommand.ExecuteIfCan();
            });
        }

        private async Task ReadNotification(Octokit.Notification notification)
        {
            if (notification == null || !notification.Unread) return;
            await _applicationService.GitHubClient.Notification.MarkAsRead(int.Parse(notification.Id));
            notification.Unread = false;
            _notifications.Remove(notification);
        }

        private void GoToNotification(Octokit.Notification x)
        {
            var subject = x.Subject.Type.ToLower();
            if (subject.Equals("issue"))
            {
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                var vm = this.CreateViewModel<IssueViewModel>();
                vm.RepositoryOwner = x.Repository.Owner.Login;
                vm.RepositoryName = x.Repository.Name;
                vm.Id = int.Parse(node);
                NavigateTo(vm);
            }
            else if (subject.Equals("pullrequest"))
            {
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                var vm = this.CreateViewModel<PullRequestViewModel>();
                vm.RepositoryOwner = x.Repository.Owner.Login;
                vm.RepositoryName = x.Repository.Name;
                vm.Id = int.Parse(node);
                NavigateTo(vm);
            }
            else if (subject.Equals("commit"))
            {
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                var vm = this.CreateViewModel<CommitViewModel>();
                vm.RepositoryOwner = x.Repository.Owner.Login;
                vm.RepositoryName = x.Repository.Name;
                vm.Node = node;
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

            ReadNotification(x);
        }
    }
}


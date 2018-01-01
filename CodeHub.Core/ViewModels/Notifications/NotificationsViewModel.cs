using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Filters;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.Changesets;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.Core.ViewModels.Notifications
{
    public class NotificationsViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IMessageService _messageService;
        private readonly FilterableCollectionViewModel<Octokit.Notification, NotificationsFilterModel> _notifications;
        private ICommand _readAllCommand;
        private ICommand _readReposCommand;
        private int _shownIndex;
        private bool _isMarking;

        public FilterableCollectionViewModel<Octokit.Notification, NotificationsFilterModel> Notifications
        {
            get { return _notifications; }
        }

        public int ShownIndex
        {
            get { return _shownIndex; }
            set { this.RaiseAndSetIfChanged(ref _shownIndex, value); }
        }

        public bool IsMarking
        {
            get { return _isMarking; }
            set { this.RaiseAndSetIfChanged(ref _isMarking, value); }
        }

        public ICommand ReadRepositoriesCommand
        {
            get { return _readReposCommand ?? (_readReposCommand = new MvxAsyncCommand<string>(x => MarkRepoAsRead(x))); }
        }

        public ICommand ReadAllCommand
        {
            get { return _readAllCommand ?? (_readAllCommand = new MvxAsyncCommand(() => MarkAllAsRead(), () => ShownIndex != 2 && !IsLoading && !IsMarking && Notifications.Any())); }
        }

        public ICommand GoToNotificationCommand
        {
            get { return new MvxCommand<Octokit.Notification>(GoToNotification); }
        }
        
        private void GoToNotification(Octokit.Notification x)
        {
            var subject = x.Subject.Type.ToLower();
            if (subject.Equals("issue"))
            {
                Read(x).ToBackground();
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                ShowViewModel<IssueViewModel>(new IssueViewModel.NavObject { Username = x.Repository.Owner.Login,Repository = x.Repository.Name, Id = long.Parse(node) });
            }
            else if (subject.Equals("pullrequest"))
            {
                Read(x).ToBackground();
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = x.Repository.Owner.Login, Repository = x.Repository.Name, Id = long.Parse(node) });
            }
            else if (subject.Equals("commit"))
            {
                Read(x).ToBackground();
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                ShowViewModel<ChangesetViewModel>(new ChangesetViewModel.NavObject { Username = x.Repository.Owner.Login, Repository = x.Repository.Name, Node = node });
            }
            else if (subject.Equals("release"))
            {
                Read(x).ToBackground();
                ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Repository.Owner.Login, Repository = x.Repository.Name });
            }
        }

        public NotificationsViewModel(
            IMessageService messageService = null,
            IApplicationService applicationService = null)
        {
            _messageService = messageService ?? GetService<IMessageService>();
            _applicationService = applicationService ?? GetService<IApplicationService>();
            _notifications = new FilterableCollectionViewModel<Octokit.Notification, NotificationsFilterModel>("Notifications");
            _notifications.GroupingFunction = (n) => n.GroupBy(x => x.Repository.FullName);
            _notifications.Bind(x => x.Filter).Subscribe(_ => LoadCommand.Execute(false));

            this.Bind(x => x.ShownIndex).Subscribe(x => {
                if (x == 0) _notifications.Filter = NotificationsFilterModel.CreateUnreadFilter();
                else if (x == 1) _notifications.Filter = NotificationsFilterModel.CreateParticipatingFilter();
                else _notifications.Filter = NotificationsFilterModel.CreateAllFilter();
                ((IMvxCommand)ReadAllCommand).RaiseCanExecuteChanged();
            });
            this.Bind(x => x.IsLoading).Subscribe(_ => ((IMvxCommand)ReadAllCommand).RaiseCanExecuteChanged());

            if (_notifications.Filter.Equals(NotificationsFilterModel.CreateUnreadFilter()))
                _shownIndex = 0;
            else if (_notifications.Filter.Equals(NotificationsFilterModel.CreateParticipatingFilter()))
                _shownIndex = 1;
            else
                _shownIndex = 2;

        }

        protected override async Task Load()
        {
            var req = new Octokit.NotificationsRequest
            {
                All = Notifications.Filter.All,
                Participating = Notifications.Filter.Participating
            };

            var notifications = await _applicationService.GitHubClient.Activity.Notifications.GetAllForCurrent(req);
            Notifications.Items.Reset(notifications);
            UpdateAccountNotificationsCount();
        }

        private async Task Read(Octokit.Notification model)
        {
            // If its already read, ignore it
            if (!model.Unread)
                return;

            try
            {
                if (!int.TryParse(model.Id, out int id))
                    return;

                await _applicationService.GitHubClient.Activity.Notifications.MarkAsRead(id);

                if (_shownIndex != 2)
                    Notifications.Items.Remove(model);

                UpdateAccountNotificationsCount();
            }
            catch
            {
                DisplayAlert("Unable to mark notification as read. Please try again.");
            }
        }

        private async Task MarkRepoAsRead(string repo)
        {
            try
            {
                IsMarking = true;
                var repoId = RepositoryIdentifier.FromFullName(repo);
                if (repoId == null) return;
                await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Notifications.MarkRepoAsRead(repoId.Owner, repoId.Name));
                Notifications.Items.RemoveRange(Notifications.Items.Where(x => string.Equals(x.Repository.FullName, repo, StringComparison.OrdinalIgnoreCase)).ToList());
                UpdateAccountNotificationsCount();
            }
            catch
            {
                DisplayAlert("Unable to mark repositories' notifications as read. Please try again.");
            }
            finally
            {
                IsMarking = false;
            }
        }

        private async Task MarkAllAsRead()
        {
            // Make sure theres some sort of notification
            if (!Notifications.Any())
                return;

            try
            {
                IsMarking = true;
                await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Notifications.MarkAsRead());
                Notifications.Items.Clear();
                UpdateAccountNotificationsCount();
            }
            catch
            {
                DisplayAlert("Unable to mark all notifications as read. Please try again.");
            }
            finally
            {
                IsMarking = false;
            }
        }

        private void UpdateAccountNotificationsCount()
        {
            // Only update if we're looking at 
            if (!Notifications.Filter.All && !Notifications.Filter.Participating)
            {
                var count = Notifications.Items.Sum(x => x.Unread ? 1 : 0);
                _messageService.Send(new NotificationCountMessage(count));
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using CodeHub.Core.Data;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.App
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IFeaturesService _featuresService;
        private int _notifications;
        private IReadOnlyList<Octokit.Organization> _organizations;
        private readonly IDisposable _notificationCountToken;

        public int Notifications
        {
            get { return _notifications; }
            set { this.RaiseAndSetIfChanged(ref _notifications, value); }
        }

        public IReadOnlyList<Octokit.Organization> Organizations
        {
            get { return _organizations; }
            set { this.RaiseAndSetIfChanged(ref _organizations, value); }
        }

        public Account Account => _applicationService.Account;

        public bool ShouldShowUpgrades => !_featuresService.IsProEnabled;

        public IEnumerable<PinnedRepository> PinnedRepositories => Account.PinnedRepositories;

        public ReactiveCommand<Unit, Unit> LoadCommand;

        public ReactiveCommand<PinnedRepository, Unit> DeletePinnedRepositoryCommand;
        
        public MenuViewModel(IApplicationService application = null,
                             IFeaturesService featuresService = null,
                             IMessageService messageService = null)
        {
            _applicationService = application ?? GetService<IApplicationService>();
            _featuresService = featuresService ?? GetService<IFeaturesService>();
            messageService = messageService ?? GetService<IMessageService>();

            _notificationCountToken = messageService.Listen<NotificationCountMessage>(OnNotificationCountMessage);

            LoadCommand = ReactiveCommand.CreateFromTask(
                () => Task.WhenAll(LoadNotifications(), LoadOrganizations()));

            DeletePinnedRepositoryCommand = ReactiveCommand.Create<PinnedRepository>(x =>
            {
                Account.PinnedRepositories.Remove(x);
                _applicationService.UpdateActiveAccount().ToBackground();
            });
        }

        private void OnNotificationCountMessage(NotificationCountMessage msg)
        {
            Notifications = msg.Count;
        }

        private async Task LoadNotifications()
        {
            var result = await _applicationService.GitHubClient.Activity.Notifications.GetAllForCurrent();
            Notifications = result.Count;
        }

        private async Task LoadOrganizations()
        {
            Organizations = await _applicationService.GitHubClient.Organization.GetAllForCurrent();
        }
    }
}

using System.Collections.Generic;
using System.Windows.Input;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.User;
using System.Linq;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.Notifications;
using GitHubSharp.Models;
using MvvmCross.Core.ViewModels;
using System;

namespace CodeHub.Core.ViewModels.App
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IFeaturesService _featuresService;
        private int _notifications;
        private List<BasicUserModel> _organizations;
        private readonly IDisposable _notificationCountToken;

        public int Notifications
        {
            get { return _notifications; }
            set { _notifications = value; RaisePropertyChanged(); }
        }

        public List<BasicUserModel> Organizations
        {
            get { return _organizations; }
            set { _organizations = value; RaisePropertyChanged(); }
        }
        
        public Account Account
        {
            get { return _applicationService.Account; }
        }

        public bool ShouldShowUpgrades
        {
            get { return !_featuresService.IsProEnabled; }
        }
        
        public MenuViewModel(IApplicationService application = null,
                             IFeaturesService featuresService = null,
                             IMessageService messageService = null)
        {
            _applicationService = application ?? GetService<IApplicationService>();
            _featuresService = featuresService ?? GetService<IFeaturesService>();
            messageService = messageService ?? GetService<IMessageService>();

            _notificationCountToken = messageService.Listen<NotificationCountMessage>(OnNotificationCountMessage);
        }

        private void OnNotificationCountMessage(NotificationCountMessage msg)
        {
            Notifications = msg.Count;
        }

        public ICommand GoToProfileCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserViewModel>(new UserViewModel.NavObject { Username = _applicationService.Account.Username })); }
        }

        public ICommand GoToNotificationsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<NotificationsViewModel>(null)); }
        }

        public ICommand GoToMyIssuesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<MyIssuesViewModel>(null)); }
        }

        public ICommand GoToMyEvents
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserEventsViewModel>(new UserEventsViewModel.NavObject { Username = Account.Username })); }
        }

        public ICommand GoToOrganizationEventsCommand
        {
            get { return new MvxCommand<string>(x => ShowMenuViewModel<Events.UserEventsViewModel>(new Events.UserEventsViewModel.NavObject { Username = x }));}
        }

        public ICommand GoToOrganizationCommand
        {
            get { return new MvxCommand<string>(x => ShowMenuViewModel<Organizations.OrganizationViewModel>(new Organizations.OrganizationViewModel.NavObject { Name = x }));}
        }

        public ICommand GoToOrganizationsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<Organizations.OrganizationsViewModel>(new Organizations.OrganizationsViewModel.NavObject { Username = Account.Username }));}
        }

        public ICommand GoToNewsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<NewsViewModel>(null));}
        }

        public ICommand LoadCommand
        {
            get { return new MvxCommand(Load);}    
        }

        private void Load()
        {
            var notificationRequest = this.GetApplication().Client.Notifications.GetAll();
            this.GetApplication().Client.ExecuteAsync(notificationRequest)
                .ToBackground(x => Notifications = x.Data.Count);

            var organizationsRequest = this.GetApplication().Client.AuthenticatedUser.GetOrganizations();
            this.GetApplication().Client.ExecuteAsync(organizationsRequest)
                .ToBackground(x => Organizations = x.Data.ToList());
        }

        //
        //        private async Task PromptForPushNotifications()
        //        {
        //            // Push notifications are not enabled for enterprise
        //            if (Account.IsEnterprise)
        //                return;
        //
        //            try
        //            {
        //                var features = Mvx.Resolve<IFeaturesService>();
        //                var alertDialog = Mvx.Resolve<IAlertDialogService>();
        //                var push = Mvx.Resolve<IPushNotificationsService>();
        //                var 
        //                // Check for push notifications
        //                if (Account.IsPushNotificationsEnabled == null && features.IsPushNotificationsActivated)
        //                {
        //                    var result = await alertDialog.PromptYesNo("Push Notifications", "Would you like to enable push notifications for this account?");
        //                    if (result)
        //                        Task.Run(() => push.Register()).FireAndForget();
        //                    Account.IsPushNotificationsEnabled = result;
        //                    Accounts.Update(Account);
        //                }
        //                else if (Account.IsPushNotificationsEnabled.HasValue && Account.IsPushNotificationsEnabled.Value)
        //                {
        //                    Task.Run(() => push.Register()).FireAndForget();
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                _alertDialogService.Alert("Error", e.Message);
        //            }
        //        }

        private static readonly IDictionary<string, string> Presentation = new Dictionary<string, string> { { PresentationValues.SlideoutRootPresentation, string.Empty } };

        public ICommand DeletePinnedRepositoryCommand
        {
            get
            {
                return new MvxCommand<PinnedRepository>(x =>
                {
                    Account.PinnedRepositories.Remove(x);
                    _applicationService.UpdateActiveAccount().ToBackground();
                }, x => x != null);
            }
        }

        protected bool ShowMenuViewModel<T>(object data) where T : IMvxViewModel
        {
            return this.ShowViewModel<T>(data, new MvxBundle(Presentation));
        }

        public IEnumerable<PinnedRepository> PinnedRepositories
        {
            get { return Account.PinnedRepositories; }
        }
    }
}

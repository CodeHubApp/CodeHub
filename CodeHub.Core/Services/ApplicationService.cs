using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using GitHubSharp;
using System.Threading.Tasks;
using System;
using MvvmCross.Core.Views;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IMvxViewDispatcher _viewDispatcher;
        private readonly IPushNotificationsService _pushNotifications;
        private readonly IFeaturesService _features;
        private readonly IAlertDialogService _alertDialogService;

        public Client Client { get; private set; }
        public GitHubAccount Account { get; private set; }
        public IAccountsService Accounts { get; private set; }

		public Action ActivationAction { get; set; }

        public ApplicationService(IAccountsService accounts, IMvxViewDispatcher viewDispatcher, 
            IFeaturesService features, IPushNotificationsService pushNotifications,
            IAlertDialogService alertDialogService)
        {
            _viewDispatcher = viewDispatcher;
            _pushNotifications = pushNotifications;
            Accounts = accounts;
            _features = features;
            _alertDialogService = alertDialogService;
        }

        public void DeactivateUser()
        {
            Accounts.SetActiveAccount(null);
            Accounts.SetDefault(null);
            Account = null;
            Client = null;
        }

        public void ActivateUser(GitHubAccount account, Client client)
        {
            Accounts.SetActiveAccount(account);
            Account = account;
            Client = client;

            //Set the default account
            Accounts.SetDefault(account);

            // Show the menu & show a page on the slideout
            _viewDispatcher.ShowViewModel(new MvxViewModelRequest {ViewModelType = typeof (MenuViewModel)});

            //Activate push notifications
            PromptForPushNotifications();
        }

        public void SetUserActivationAction(Action action)
        {
            if (Account != null)
                action();
            else
				ActivationAction = action;
        }

        private async Task PromptForPushNotifications()
        {
            // Push notifications are not enabled for enterprise
            if (Account.IsEnterprise)
                return;

            try
            {
                // Check for push notifications
                if (Account.IsPushNotificationsEnabled == null && _features.IsPushNotificationsActivated)
                {
                    var result = await _alertDialogService.PromptYesNo("Push Notifications", "Would you like to enable push notifications for this account?");
                    if (result)
                        Task.Run(() => _pushNotifications.Register()).FireAndForget();
                    Account.IsPushNotificationsEnabled = result;
                    Accounts.Update(Account);
                }
                else if (Account.IsPushNotificationsEnabled.HasValue && Account.IsPushNotificationsEnabled.Value)
                {
                    Task.Run(() => _pushNotifications.Register()).FireAndForget();
                }
            }
            catch (Exception e)
            {
                _alertDialogService.Alert("Error", e.Message);
            }
        }
    }
}

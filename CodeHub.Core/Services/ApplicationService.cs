using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using CodeFramework.Core;
using CodeFramework.Core.Services;
using CodeHub.Core.Data;
using CodeHub.Core.ViewModels;
using GitHubSharp;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IMvxViewDispatcher _viewDispatcher;
        public Client Client { get; private set; }
        public GitHubAccount Account { get; private set; }
        public IAccountsService Accounts { get; private set; }

        public ApplicationService(IAccountsService accounts, IMvxViewDispatcher viewDispatcher)
        {
            _viewDispatcher = viewDispatcher;
            Accounts = accounts;
        }

        public void ActivateUser(GitHubAccount account, Client client)
        {
            Accounts.SetActiveAccount(account);
            Account = account;
            Client = client;

            // Show the menu & show a page on the slideout
            _viewDispatcher.ShowViewModel(new MvxViewModelRequest {ViewModelType = typeof (MenuViewModel)});
        }
    }
}

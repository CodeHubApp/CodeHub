using System;
using CodeFramework.Core.Data;
using CodeFramework.Core.Services;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels
{
    public class AccountsViewModel : BaseAccountsViewModel
    {
        private readonly ILoginService _loginService;
        private readonly IApplicationService _applicationService;

        public AccountsViewModel(IAccountsService accountsService, ILoginService loginService, IApplicationService applicationService) 
            : base(accountsService)
        {
            _loginService = loginService;
            _applicationService = applicationService;
        }

        protected override void AddAccount()
        {
            this.ShowViewModel<NewAccountViewModel>();
        }

        protected override void SelectAccount(IAccount account)
        {
            var githubAccount = (GitHubAccount) account;
            var client = _loginService.LoginAccount(githubAccount);
            _applicationService.ActivateUser(githubAccount, client);
        }
    }
}

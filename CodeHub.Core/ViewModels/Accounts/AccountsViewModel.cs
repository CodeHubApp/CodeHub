using CodeFramework.Core.Data;
using CodeFramework.Core.Services;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System;

namespace CodeHub.Core.ViewModels.Accounts
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

		protected async override void SelectAccount(IAccount account)
        {
            var githubAccount = (GitHubAccount) account;

			try
			{
				IsLoggingIn = true;
				var client = await _loginService.LoginAccount(githubAccount);
				_applicationService.ActivateUser(githubAccount, client);
			}
			catch (Exception e)
			{
				Error = e;
			}
			finally
			{
				IsLoggingIn = false;
			}
        }
    }
}

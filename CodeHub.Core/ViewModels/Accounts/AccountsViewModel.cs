using CodeFramework.Core.Data;
using CodeFramework.Core.Services;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System;
using CodeHub.Core.Factories;
using System.Threading.Tasks;
using System.Net;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AccountsViewModel : CodeFramework.Core.ViewModels.Application.AccountsViewModel
    {
        private readonly ILoginFactory _loginFactory;
        private readonly IApplicationService _applicationService;
		
        public AccountsViewModel(IAccountsService accountsService, ILoginFactory loginFactory, IApplicationService applicationService) 
            : base(accountsService)
        {
            _loginFactory = loginFactory;
            _applicationService = applicationService;
        }

        protected override void AddAccount()
        {
            this.ShowViewModel<NewAccountViewModel>();
        }

		protected async override void SelectAccount(IAccount account)
        {
            var githubAccount = (GitHubAccount) account;
			var isEnterprise = githubAccount.IsEnterprise || !string.IsNullOrEmpty(githubAccount.Password);

			if (githubAccount.DontRemember)
			{
				//Hack for now
				if (isEnterprise)
				{
					ShowViewModel<AddAccountViewModel>(new AddAccountViewModel.NavObject { IsEnterprise = true, AttemptedAccountId = account.Id });
				}
				else
				{
					ShowViewModel<LoginViewModel>(LoginViewModel.NavObject.CreateDontRemember(githubAccount));
				}

				return;
			}

			try
			{
				IsLoggingIn = true;
				var client = await _loginFactory.LoginAccount(githubAccount);
				_applicationService.ActivateUser(githubAccount, client);
			}
			catch (GitHubSharp.UnauthorizedException e)
			{
                DisplayAlert("The credentials for the selected account are incorrect. " + e.Message);

				if (isEnterprise)
					ShowViewModel<AddAccountViewModel>(new AddAccountViewModel.NavObject { IsEnterprise = true, AttemptedAccountId = githubAccount.Id });
				else
					ShowViewModel<LoginViewModel>(LoginViewModel.NavObject.CreateDontRemember(githubAccount));
			}
			catch (Exception e)
			{
                DisplayAlert(e.Message);
			}
			finally
			{
				IsLoggingIn = false;
			}
        }
    }
}

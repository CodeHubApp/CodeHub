using System;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Data;
using CodeFramework.Core.Services;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.App
{
	public class StartupViewModel : BaseStartupViewModel
    {
		private readonly ILoginService _loginService;
		private readonly IApplicationService _applicationService;

		public StartupViewModel(IAccountsService accountsService, ILoginService loginService, IApplicationService applicationService)
		{
			_loginService = loginService;
			_applicationService = applicationService;
		}

		protected async override void Startup()
		{
			var account = GetDefaultAccount();
			if (account == null)
			{
				this.ShowViewModel<Accounts.AccountsViewModel>();
				return;
			}

			//Lets login!
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


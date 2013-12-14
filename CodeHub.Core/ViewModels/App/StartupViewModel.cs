using System;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System.Linq;

namespace CodeHub.Core.ViewModels.App
{
	public class StartupViewModel : BaseStartupViewModel
    {
		private readonly ILoginService _loginService;
		private readonly IApplicationService _applicationService;

		public StartupViewModel(ILoginService loginService, IApplicationService applicationService)
		{
			_loginService = loginService;
			_applicationService = applicationService;
		}

		protected async override void Startup()
		{
			if (!_applicationService.Accounts.Any())
			{
				ShowViewModel<Accounts.AccountsViewModel>();
				ShowViewModel<Accounts.NewAccountViewModel>();
				return;
			}

			var account = GetDefaultAccount() as GitHubAccount;
			if (account == null)
			{
				ShowViewModel<Accounts.AccountsViewModel>();
				return;
			}

			if (account.DontRemember)
			{
				ShowViewModel<Accounts.AccountsViewModel>();

				//Hack for now
				if (account.IsEnterprise || !string.IsNullOrEmpty(account.Password))
				{
					ShowViewModel<Accounts.AddAccountViewModel>(new Accounts.AddAccountViewModel.NavObject { IsEnterprise = true, AttemptedAccountId = account.Id });
				}
				else
				{
					ShowViewModel<Accounts.LoginViewModel>(Accounts.LoginViewModel.NavObject.CreateDontRemember(account));
				}

				return;
			}

			//Lets login!
			try
			{
				IsLoggingIn = true;
				var client = await _loginService.LoginAccount(account);
				_applicationService.ActivateUser(account, client);
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


using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System;
using CodeHub.Core.Factories;
using System.Threading.Tasks;
using System.Net;
using CodeHub.Core.Utils;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels.App;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AccountsViewModel : BaseViewModel
    {
        private readonly IAccountsService _accountsService;
        private readonly CustomObservableCollection<IAccount> _accounts = new CustomObservableCollection<IAccount>();
        private bool _isLoggingIn;

        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            protected set
            {
                _isLoggingIn = value;
                RaisePropertyChanged(() => IsLoggingIn);
            }
        }

        public CustomObservableCollection<IAccount> Accounts
        {
            get { return _accounts; }
        }

        public IMvxCommand AddAccountCommand
        {
            get { return new MvxCommand(() => ShowViewModel<NewAccountViewModel>()); }
        }

        public IMvxCommand SelectAccountCommand
        {
            get { return new MvxCommand<GitHubAccount>(SelectAccount); }
        }

        public void Init()
        {
            _accounts.Reset(_accountsService);
        }
		
        public AccountsViewModel(IAccountsService accountsService) 
        {
            _accountsService = accountsService;
        }

        private void SelectAccount(GitHubAccount githubAccount)
        {
			var isEnterprise = githubAccount.IsEnterprise || !string.IsNullOrEmpty(githubAccount.Password);

			if (githubAccount.DontRemember)
			{
				//Hack for now
				if (isEnterprise)
				{
                    ShowViewModel<AddAccountViewModel>(new AddAccountViewModel.NavObject { AttemptedAccountId = githubAccount.Id });
				}
				else
				{
					ShowViewModel<LoginViewModel>(LoginViewModel.NavObject.CreateDontRemember(githubAccount));
				}

				return;
			}

            _accountsService.SetDefault(githubAccount);
            ShowViewModel<StartupViewModel>();
        }
    }
}

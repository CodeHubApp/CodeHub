using System;
using System.Reactive.Linq;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.Accounts;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.App
{
    public class AccountsViewModel : BaseViewModel
    {
        private readonly IAccountsService _accountsService;

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            protected set { this.RaiseAndSetIfChanged(ref _isLoggingIn, value); }
        }

        public GitHubAccount ActiveAccount
        {
            get { return _accountsService.ActiveAccount; }
            set
            {
                _accountsService.ActiveAccount = value;
                this.RaisePropertyChanged();
            }
        }

        public ReactiveList<GitHubAccount> Accounts { get; private set; }

        public IReactiveCommand<object> LoginCommand { get; private set; }

        public IReactiveCommand<object> GoToAddAccountCommand { get; private set; }

        public IReactiveCommand<object> DeleteAccountCommand { get; private set; }

        public AccountsViewModel(IAccountsService accountsService)
        {
            _accountsService = accountsService;
            Accounts = new ReactiveList<GitHubAccount>(accountsService);
            LoginCommand = ReactiveCommand.Create();
            GoToAddAccountCommand = ReactiveCommand.Create();
            DeleteAccountCommand = ReactiveCommand.Create();

            DeleteAccountCommand.OfType<GitHubAccount>().Subscribe(x =>
            {
                if (Equals(accountsService.ActiveAccount, x))
                    ActiveAccount = null;
                accountsService.Remove(x);
                Accounts.Remove(x);
            });

            LoginCommand.OfType<GitHubAccount>().Subscribe(x =>
            {
                if (Equals(accountsService.ActiveAccount, x))
                    DismissCommand.ExecuteIfCan();
                else
                {
                    ActiveAccount = x;
                    MessageBus.Current.SendMessage(new LogoutMessage());
                    DismissCommand.ExecuteIfCan();
                }
            });

            GoToAddAccountCommand.Subscribe(_ => ShowViewModel(CreateViewModel<NewAccountViewModel>()));

            this.WhenActivated(d => Accounts.Reset(accountsService));
        }
    }
}

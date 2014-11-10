using System;
using System.Reactive.Linq;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.Accounts;
using CodeHub.Core.Services;
using System.Linq;

namespace CodeHub.Core.ViewModels.App
{
    public class AccountsViewModel : BaseViewModel
    {
        private readonly IAccountsService _accountsService;
        private readonly ReactiveList<GitHubAccount> _accounts;

        public GitHubAccount ActiveAccount
        {
            get { return _accountsService.ActiveAccount; }
            set
            {
                _accountsService.ActiveAccount = value;
                this.RaisePropertyChanged();
            }
        }

        public new IReactiveCommand DismissCommand { get; private set; }

        public IReadOnlyReactiveList<AccountItemViewModel> Accounts { get; private set; }

        public IReactiveCommand<object> LoginCommand { get; private set; }

        public IReactiveCommand<object> GoToAddAccountCommand { get; private set; }

        public IReactiveCommand<object> DeleteAccountCommand { get; private set; }

        public AccountsViewModel(IAccountsService accountsService)
        {
            _accountsService = accountsService;

            Title = "Accounts";

            _accounts = new ReactiveList<GitHubAccount>(accountsService.OrderBy(x => x.Username));
            this.WhenActivated(d => _accounts.Reset(accountsService.OrderBy(x => x.Username)));
            Accounts = _accounts.CreateDerivedCollection(CreateAccountItem);

            this.WhenAnyValue(x => x.ActiveAccount)
                .Subscribe(x =>
                {
                    foreach (var account in Accounts)
                        account.Selected = Equals(account.Account, x);
                });

            DeleteAccountCommand = ReactiveCommand.Create();
            DeleteAccountCommand.OfType<GitHubAccount>().Subscribe(x =>
            {
                if (Equals(accountsService.ActiveAccount, x))
                    ActiveAccount = null;
                accountsService.Remove(x);
                _accounts.Remove(x);
            });

            LoginCommand = ReactiveCommand.Create();
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

            DismissCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ActiveAccount).Select(x => x != null))
                                            .WithSubscription(x => base.DismissCommand.ExecuteIfCan(x));

            GoToAddAccountCommand = ReactiveCommand.Create()
                .WithSubscription(_ => ShowViewModel(CreateViewModel<NewAccountViewModel>()));

        }

        private AccountItemViewModel CreateAccountItem(GitHubAccount githubAccount)
        {
            var viewModel = new AccountItemViewModel();
            viewModel.Account = githubAccount;
            viewModel.Selected = Equals(githubAccount, ActiveAccount);
            viewModel.DeleteCommand.Subscribe(_ => DeleteAccountCommand.ExecuteIfCan(githubAccount));
            viewModel.SelectCommand.Subscribe(_ => LoginCommand.ExecuteIfCan(githubAccount));
            return viewModel;
        }
    }
}

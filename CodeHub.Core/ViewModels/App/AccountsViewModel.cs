using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using ReactiveUI;
using CodeHub.Core.Data;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.Accounts;
using CodeHub.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.App
{
    public class AccountsViewModel : BaseViewModel
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly ISessionService _sessionService;
        private readonly ReactiveList<GitHubAccount> _accounts;

        public GitHubAccount ActiveAccount
        {
            get { return _sessionService.Account; }
            private set
            {
                _sessionService.SetSessionAccount(value);
                this.RaisePropertyChanged();
            }
        }

        public IReadOnlyReactiveList<AccountItemViewModel> Accounts { get; private set; }

        public IReactiveCommand<object> GoToAddAccountCommand { get; private set; }

        public IReactiveCommand<object> DismissCommand { get; private set; }

        public AccountsViewModel(ISessionService sessionService, IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
            _sessionService = sessionService;

            Title = "Accounts";

            _accounts = new ReactiveList<GitHubAccount>();

            Accounts = _accounts.CreateDerivedCollection(CreateAccountItem);

            this.WhenAnyValue(x => x.ActiveAccount)
                .Select(x => x == null ? string.Empty : x.Key)
                .Subscribe(x =>
                {
                    foreach (var account in Accounts)
                        account.Selected = account.Id == x;
                });

            var canDismiss = this.WhenAnyValue(x => x.ActiveAccount).Select(x => x != null);
            DismissCommand = ReactiveCommand.Create(canDismiss).WithSubscription(x => Dismiss());

            GoToAddAccountCommand = ReactiveCommand.Create()
                .WithSubscription(_ => NavigateTo(this.CreateViewModel<NewAccountViewModel>()));

            UpdateAccounts();
            this.WhenActivated(d => UpdateAccounts());
        }

        private void UpdateAccounts()
        {
            _accountsRepository.GetAll().ToObservable()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => _accounts.Reset(x));
        }

        private async Task LoginAccount(GitHubAccount account)
        {
            if (!Equals(_sessionService.Account, account))
            {
                ActiveAccount = account;
                await _accountsRepository.SetDefault(account);
                MessageBus.Current.SendMessage(new LogoutMessage());
            }

            Dismiss();
        }

        private async Task DeleteAccount(GitHubAccount account)
        {
            if (Equals(_sessionService.Account, account))
                ActiveAccount = null;
            _accounts.Remove(account);
            await _accountsRepository.Remove(account);
        }

        private AccountItemViewModel CreateAccountItem(GitHubAccount githubAccount)
        {
            var viewModel = new AccountItemViewModel(githubAccount);
            viewModel.Selected = Equals(githubAccount, ActiveAccount);
            viewModel.DeleteCommand.Subscribe(_ => DeleteAccount(githubAccount));
            viewModel.GoToCommand.Subscribe(_ => LoginAccount(githubAccount));
            return viewModel;
        }
    }
}

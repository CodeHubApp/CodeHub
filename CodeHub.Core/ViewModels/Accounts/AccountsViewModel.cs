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

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AccountsViewModel : BaseViewModel, IListViewModel<AccountItemViewModel>
    {
        private readonly ReactiveList<GitHubAccount> _accounts = new ReactiveList<GitHubAccount>();
        private readonly IAccountsRepository _accountsRepository;
        private readonly ISessionService _sessionService;

        public GitHubAccount ActiveAccount
        {
            get { return _sessionService.Account; }
            private set
            {
                _sessionService.SetSessionAccount(value);
                this.RaisePropertyChanged();
            }
        }

        public IReadOnlyReactiveList<AccountItemViewModel> Items { get; }

        public IReactiveCommand<object> GoToAddAccountCommand { get; }

        public IReactiveCommand<object> DismissCommand { get; }

        public AccountsViewModel(ISessionService sessionService, IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
            _sessionService = sessionService;

            Title = "Accounts";

            Items = _accounts.CreateDerivedCollection(CreateAccountItem);

            this.WhenAnyValue(x => x.ActiveAccount)
                .Select(x => x == null ? string.Empty : x.Key)
                .Subscribe(x =>
                {
                    foreach (var account in Items)
                        account.Selected = account.Id == x;
                });

            var canDismiss = this.WhenAnyValue(x => x.ActiveAccount).Select(x => x != null);
            DismissCommand = ReactiveCommand.Create(canDismiss).WithSubscription(x => Dismiss());

            GoToAddAccountCommand = ReactiveCommand.Create()
                .WithSubscription(_ => NavigateTo(this.CreateViewModel<NewAccountViewModel>()));

            // Activate immediately since WhenActivated triggers off Did* instead of Will* in iOS
            UpdateAccounts();
            this.WhenActivated(d => UpdateAccounts());
        }

        private void UpdateAccounts()
        {
            _accountsRepository.GetAll().ToObservable()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => x.OrderBy(y => y.Username).ToList())
                .Where(x => !x.All(_accounts.Contains))
                .Subscribe(_accounts.Reset);
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

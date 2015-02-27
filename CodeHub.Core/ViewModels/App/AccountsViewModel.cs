using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using ReactiveUI;
using CodeHub.Core.Data;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.Accounts;
using CodeHub.Core.Services;
using System.Linq;

namespace CodeHub.Core.ViewModels.App
{
    public class AccountsViewModel : BaseViewModel
    {
        private readonly IAccountsRepository _accountsService;
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

        public IReactiveCommand<object> LoginCommand { get; private set; }

        public IReactiveCommand<object> GoToAddAccountCommand { get; private set; }

        public IReactiveCommand<object> DeleteAccountCommand { get; private set; }

        public IReactiveCommand<object> DismissCommand { get; private set; }

        public AccountsViewModel(ISessionService sessionService, IAccountsRepository accountsService)
        {
            _accountsService = accountsService;
            _sessionService = sessionService;

            Title = "Accounts";

            _accounts = new ReactiveList<GitHubAccount>();
            Accounts = _accounts.CreateDerivedCollection(CreateAccountItem);

            this.WhenAnyValue(x => x.ActiveAccount)
                .Subscribe(x =>
                {
                    foreach (var account in Accounts)
                        account.Selected = account.Id == x.Key;
                });

            DeleteAccountCommand = ReactiveCommand.Create();
            DeleteAccountCommand.OfType<GitHubAccount>().Subscribe(x =>
            {
                if (Equals(sessionService.Account, x))
                    ActiveAccount = null;
                accountsService.Remove(x);
                _accounts.Remove(x);
            });

            LoginCommand = ReactiveCommand.Create();
            LoginCommand.OfType<GitHubAccount>().Subscribe(x =>
            {
                if (!Equals(sessionService.Account, x))
                {
                    ActiveAccount = x;
                    MessageBus.Current.SendMessage(new LogoutMessage());
                }

                Dismiss();
            });

            DismissCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ActiveAccount)
                .Select(x => x != null)).WithSubscription(x => Dismiss());

            GoToAddAccountCommand = ReactiveCommand.Create()
                .WithSubscription(_ => NavigateTo(this.CreateViewModel<NewAccountViewModel>()));

            this.WhenActivated(d => 
            {
                accountsService.GetAll().ToObservable()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => _accounts.Reset(x));
            });
        }

        private AccountItemViewModel CreateAccountItem(GitHubAccount githubAccount)
        {
            var viewModel = new AccountItemViewModel(githubAccount);
            viewModel.Selected = Equals(githubAccount, ActiveAccount);
            viewModel.DeleteCommand.Subscribe(_ => DeleteAccountCommand.ExecuteIfCan(githubAccount));
            viewModel.GoToCommand.Subscribe(_ => LoginCommand.ExecuteIfCan(githubAccount));
            return viewModel;
        }
    }
}

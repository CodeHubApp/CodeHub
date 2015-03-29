using System;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddEnterpriseAccountViewModel : AddAccountViewModel 
    {
        private readonly ILoginService _loginFactory;
        private readonly IAccountsRepository _accountsRepository;

        public IReactiveCommand<Unit> ShowLoginOptionsCommand { get; private set; }

        public AddEnterpriseAccountViewModel(
            ILoginService loginFactory, 
            IAccountsRepository accountsRepository,
            IAlertDialogFactory alertDialogFactory,
            IActionMenuFactory actionMenuFactory)
            : base(alertDialogFactory)
        {
            _loginFactory = loginFactory;
            _accountsRepository = accountsRepository;

            var gotoOAuthToken = ReactiveCommand.Create().WithSubscription(_ => {
                var vm = this.CreateViewModel<EnterpriseOAuthTokenLoginViewModel>();
                vm.Domain = Domain;
                NavigateTo(vm);
            });

            ShowLoginOptionsCommand = ReactiveCommand.CreateAsyncTask(sender => {
                var actionMenu = actionMenuFactory.Create(Title);
                actionMenu.AddButton("Login via Token", gotoOAuthToken);
                return actionMenu.Show(sender);
            });
        }

        protected override async Task<GitHubAccount> Login()
        {
            Uri domainUri;
            if (!Uri.TryCreate(Domain, UriKind.Absolute, out domainUri))
                throw new Exception("The provided domain is not a valid URL.");

            var apiUrl = Domain;
            if (apiUrl != null)
            {
                if (!apiUrl.EndsWith("/", StringComparison.Ordinal))
                    apiUrl += "/";
                if (!apiUrl.Contains("/api/"))
                    apiUrl += "api/v3/";
            }

            var account = await _loginFactory.Authenticate(apiUrl, Domain, Username, Password, TwoFactor, true);
            await _accountsRepository.SetDefault(account);
            return account;
        }

    }
}

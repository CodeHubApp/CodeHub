using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddWebAccountViewModel : AddAccountViewModel 
    {
        private const string WebDomain = "https://github.com";
        private const string ApiDomain = "https://api.github.com";
        private readonly ILoginService _loginFactory;
        private readonly IAccountsRepository _accountsService;

        public AddWebAccountViewModel(
            ILoginService loginFactory, 
            IAccountsRepository accountsService,
            IAlertDialogFactory alertDialogFactory)
            : base(alertDialogFactory)
        {
            _loginFactory = loginFactory;
            _accountsService = accountsService;
            Domain = WebDomain;
        }

        protected override async Task<GitHubAccount> Login()
        {
            var account = await _loginFactory.Authenticate(ApiDomain, Domain, Username, Password, TwoFactor, false);
            await _accountsService.SetDefault(account);
            return account;
        }
    }
}

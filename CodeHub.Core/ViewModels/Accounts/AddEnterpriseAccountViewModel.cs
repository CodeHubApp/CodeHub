using System;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddEnterpriseAccountViewModel : AddAccountViewModel 
    {
        private readonly ILoginService _loginFactory;
        private readonly IAccountsRepository _accountsService;

        public AddEnterpriseAccountViewModel(
            ILoginService loginFactory, 
            IAccountsRepository accountsService,
            IAlertDialogFactory alertDialogFactory)
            : base(alertDialogFactory)
        {
            _loginFactory = loginFactory;
            _accountsService = accountsService;
        }

        protected override async Task<GitHubAccount> Login()
        {
            var apiUrl = Domain;
            if (apiUrl != null)
            {
                if (!apiUrl.StartsWith("http://", StringComparison.Ordinal) && !apiUrl.StartsWith("https://", StringComparison.Ordinal))
                    apiUrl = "https://" + apiUrl;
                if (!apiUrl.EndsWith("/", StringComparison.Ordinal))
                    apiUrl += "/";
                if (!apiUrl.Contains("/api/"))
                    apiUrl += "api/v3/";
            }

            var account = await _loginFactory.Authenticate(apiUrl, Domain, Username, Password, TwoFactor, true);
            await _accountsService.SetDefault(account);
            return account;
        }
    }
}

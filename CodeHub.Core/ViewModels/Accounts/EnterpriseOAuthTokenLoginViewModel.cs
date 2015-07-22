using System;
using CodeHub.Core.Data;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.Messages;
using CodeHub.Core.Factories;
using GitHubSharp;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class EnterpriseOAuthTokenLoginViewModel : BaseViewModel
    {
        public IReactiveCommand<GitHubAccount> LoginCommand { get; private set; }

        private string _token;
        public string Token
        {
            get { return _token; }
            set { this.RaiseAndSetIfChanged(ref _token, value); }
        }

        private string _domain;
        public string Domain
        {
            get { return _domain; }
            set { this.RaiseAndSetIfChanged(ref _domain, value); }
        }

        public EnterpriseOAuthTokenLoginViewModel(
            ILoginService loginFactory, 
            IAccountsRepository accountsRepository,
            IAlertDialogFactory alertDialogFactory)
        {
            Title = "Login";

            LoginCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                if (string.IsNullOrEmpty(Domain))
                    throw new ArgumentException("Must have a valid GitHub domain");
                if (string.IsNullOrEmpty(Token))
                    throw new ArgumentException("Must have a valid Token");
                
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

                try
                {
                    using (alertDialogFactory.Activate("Logging in..."))
                    {
                        var account = await loginFactory.Authenticate(apiUrl, Domain, Token, true);
                        await accountsRepository.SetDefault(account);
                        return account;
                    }
                }
                catch (UnauthorizedException)
                {
                    throw new Exception("The provided token is invalid! Please try again or " +
                        "create a new token as this one might have been revoked.");
                }
            });

            LoginCommand.Subscribe(x => MessageBus.Current.SendMessage(new LogoutMessage()));
        }
    }
}


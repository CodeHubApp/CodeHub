using System;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.Messages;
using GitHubSharp;
using System.Reactive.Linq;
using System.Reactive;
using Splat;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class EnterpriseOAuthTokenLoginViewModel : BaseViewModel
    {
        public ReactiveCommand<Unit, Unit> LoginCommand { get; private set; }

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
            ILoginService loginService = null,
            IAlertDialogService alertDialogService = null)
        {
            loginService = loginService ?? Locator.Current.GetService<ILoginService>();
            alertDialogService = alertDialogService ?? Locator.Current.GetService<IAlertDialogService>();

            Title = "Login";

            LoginCommand = ReactiveCommand.CreateFromTask(async _ => {
                if (string.IsNullOrEmpty(Domain))
                    throw new ArgumentException("Must have a valid GitHub domain");
                if (string.IsNullOrEmpty(Token))
                    throw new ArgumentException("Must have a valid Token");

                if (!Uri.TryCreate(Domain, UriKind.Absolute, out Uri domainUri))
                    throw new Exception("The provided domain is not a valid URL.");

                var apiUrl = Domain;
                if (apiUrl != null)
                {
                    if (!apiUrl.EndsWith("/", StringComparison.Ordinal))
                        apiUrl += "/";
                    if (!apiUrl.Contains("/api/"))
                        apiUrl += "api/v3/";
                }

                using (alertDialogService.Activate("Logging in..."))
                    await loginService.LoginWithToken(apiUrl, Domain, Token, true);
            });

            LoginCommand
                .ThrownExceptions
                .Select(HandleLoginException)
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            LoginCommand
                .Subscribe(x => MessageBus.Current.SendMessage(new LogoutMessage()));
        }

        private UserError HandleLoginException(Exception e)
        {
            if (e is UnauthorizedException authException)
            {
                return new UserError("The provided token is invalid! Please try again or " +
                                     "create a new token as this one might have been revoked.");
            }

            return new UserError("Unable to login!", e);
        }

    }
}


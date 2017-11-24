using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.Messages;
using System.Reactive;
using ReactiveUI;
using Splat;
using System.Reactive.Linq;
using GitHubSharp;
using System.Reactive.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddAccountViewModel : ReactiveObject 
    {
        private readonly ILoginService _loginService;
        private readonly IAlertDialogService _alertDialogService;

        private string _username;
        public string Username
        {
            get { return _username; }
            set { this.RaiseAndSetIfChanged(ref _username, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { this.RaiseAndSetIfChanged(ref _password, value); }
        }

        private string _domain;
        public string Domain
        {
            get { return _domain; }
            set { this.RaiseAndSetIfChanged(ref _domain, value); }
        }

        private string _token;
        public string Token
        {
            get { return _token; }
            set { this.RaiseAndSetIfChanged(ref _token, value); }
        }

        private bool _tokenAuthentication;
        public bool TokenAuthentication
        {
            get { return _tokenAuthentication; }
            set { this.RaiseAndSetIfChanged(ref _tokenAuthentication, value); }
        }

        public string TwoFactor { get; set; }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        public AddAccountViewModel(
            ILoginService loginService = null,
            IAlertDialogService alertDialogService = null)
        {
            _loginService = loginService ?? Locator.Current.GetService<ILoginService>();
            _alertDialogService = alertDialogService ?? Locator.Current.GetService<IAlertDialogService>();

            LoginCommand = ReactiveCommand.CreateFromTask(Login);

            LoginCommand
                .Subscribe(x => MessageBus.Current.SendMessage(new LogoutMessage()));

            LoginCommand
                .ThrownExceptions
                .SelectMany(HandleLoginException)
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();
        }

        private IObservable<UserError> HandleLoginException(Exception e)
        {
            TwoFactor = null;

            if (e is Octokit.TwoFactorRequiredException)
            {
                _alertDialogService
                    .PromptTextBox("Authentication Error",
                                   "Please provide the two-factor authentication code for this account.",
                                   string.Empty, "Login")
                    .ToObservable()
                    .Do(x => TwoFactor = x)
                    .Select(_ => Unit.Default)
                    .InvokeReactiveCommand(LoginCommand);

                return Observable.Empty<UserError>();
            }

            if (e is Octokit.NotFoundException err)
            {
                return Observable.Return(
                    new UserError($"The provided domain was incorrect. The host could not be found."));
            }

            if (e is Octokit.ForbiddenException && TokenAuthentication)
            {
                return Observable.Return(
                    new UserError("The provided token is invalid! Please try again or " +
                                  "create a new token as this one might have been revoked."));
            }

            return Observable.Return(new UserError("Unable to login!", e));
        }

        private async Task Login()
        {
            if (string.IsNullOrEmpty(Domain))
                throw new ArgumentException("Must have a valid GitHub domain");
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

            if (TokenAuthentication)
            {
                var trimmedToken = Token?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(trimmedToken))
                    throw new ArgumentException("Must have a valid token");

                await _loginService.LoginWithToken(apiUrl, Domain, trimmedToken, true);
            }
            else
            {
                if (string.IsNullOrEmpty(Username))
                    throw new ArgumentException("Must have a valid username");
                if (string.IsNullOrEmpty(Password))
                    throw new ArgumentException("Must have a valid password");
                
                await _loginService.LoginWithBasic(apiUrl, Username, Password, TwoFactor);
            }
        }
    }
}

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

        public string TwoFactor { get; set; }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        public AddAccountViewModel(
            ILoginService loginService = null,
            IAlertDialogService alertDialogService = null)
        {
            _loginService = loginService ?? Locator.Current.GetService<ILoginService>();
            _alertDialogService = alertDialogService ?? Locator.Current.GetService<IAlertDialogService>();

            var canLogin = this
                .WhenAnyValue(x => x.Username, x => x.Password)
                .Select(x => !String.IsNullOrEmpty(x.Item1) && !String.IsNullOrEmpty(x.Item2));

            LoginCommand = ReactiveCommand.CreateFromTask(Login, canLogin);

            LoginCommand.ThrownExceptions.Subscribe(HandleLoginException);
        }

        private void HandleLoginException(Exception e)
        {
            if (e is UnauthorizedException authException && authException.Headers.Contains("X-GitHub-OTP"))
            {
                _alertDialogService
                    .PromptTextBox("Authentication Error", "Please provide the two-factor authentication code for this account.", string.Empty, "Login")
                    .ToObservable()
                    .Do(x => TwoFactor = x)
                    .InvokeCommand(LoginCommand);
            }
            else
            {
                _alertDialogService
                    .Alert("Unable to Login!", "Unable to login user " + Username + ": " + e.Message)
                    .ToBackground();
            }
        }

        private async Task Login()
        {
            var apiUrl = Domain;
            if (apiUrl != null)
            {
                if (!apiUrl.StartsWith("http://") && !apiUrl.StartsWith("https://"))
                    apiUrl = "https://" + apiUrl;
                if (!apiUrl.EndsWith("/"))
                    apiUrl += "/";
                  if (!apiUrl.Contains("/api/"))
                    apiUrl += "api/v3/";
            }

            try
            {
                await _loginService.LoginWithBasic(apiUrl, Username, Password, TwoFactor);
                MessageBus.Current.SendMessage(new LogoutMessage());
            }
            catch
            {
                TwoFactor = null;
                throw;
            }
        }
    }
}

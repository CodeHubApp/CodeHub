using System;
using System.Reactive.Linq;
using CodeHub.Core.Data;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.Messages;
using CodeHub.Core.Factories;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Accounts
{
    public abstract class AddAccountViewModel : BaseViewModel 
    {
        private readonly IAlertDialogFactory _alertDialogFactory;

        public string TwoFactor { get; set; }

        public IReactiveCommand<GitHubAccount> LoginCommand { get; private set; }

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

        private GitHubAccount _attemptedAccount;
        public GitHubAccount AttemptedAccount
        {
            get { return _attemptedAccount; }
            set { this.RaiseAndSetIfChanged(ref _attemptedAccount, value); }
        }

        protected AddAccountViewModel(IAlertDialogFactory alertDialogFactory)
        {
            _alertDialogFactory = alertDialogFactory;

            Title = "Login";

            this.WhenAnyValue(x => x.AttemptedAccount).Where(x => x != null).Subscribe(x =>
            {
                Username = x.Username;
                Domain = x.Domain;
            });

            LoginCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                if (string.IsNullOrEmpty(Username))
                    throw new ArgumentException("Must have a valid username!");
                if (string.IsNullOrEmpty(Password))
                    throw new ArgumentException("Must have a valid password!");
                if (string.IsNullOrEmpty(Domain))
                    throw new ArgumentException("Must have a valid GitHub domain!");
                
                using (alertDialogFactory.Activate("Logging in..."))
                    return await Login();
            });

            LoginCommand.ThrownExceptions.Subscribe(x =>
            {
                if (x is LoginService.TwoFactorRequiredException)
                    PromptForTwoFactor().ToBackground();
                else
                    _alertDialogFactory.Alert("Error", x.Message).ToBackground();
            });

            LoginCommand.Subscribe(x => MessageBus.Current.SendMessage(new LogoutMessage()));
        }

        private async Task PromptForTwoFactor()
        {
            var result = await _alertDialogFactory.PromptTextBox("Two Factor Authentication",
                "This account requires a two factor authentication code", string.Empty, "Ok");
            TwoFactor = result;
            LoginCommand.ExecuteIfCan();
        }

        protected abstract Task<GitHubAccount> Login();
    }
}

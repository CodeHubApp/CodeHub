using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.Messages;
using System.Reactive;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddAccountViewModel : ReactiveObject 
    {
        private readonly IApplicationService _application;

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            set { this.RaiseAndSetIfChanged(ref _isLoggingIn, value); }
        }

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

        public AddAccountViewModel(IApplicationService application = null)
        {
            _application = application ?? Locator.Current.GetService<IApplicationService>();
            LoginCommand = ReactiveCommand.CreateFromTask(Login);
        }

        private async Task Login()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                return;

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
                IsLoggingIn = true;
                var account = await _application.LoginWithBasic(apiUrl, Username, Password, TwoFactor);
                var client = await _application.LoginAccount(account);
                _application.ActivateUser(account, client);
                MessageBus.Current.SendMessage(new LogoutMessage());
            }
            catch (Exception)
            {
                TwoFactor = null;
                throw;
            }
            finally
            {
                IsLoggingIn = false;
            }
        }
    }
}

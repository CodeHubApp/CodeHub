using System;
using System.Threading.Tasks;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Factories;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Messages;
using GitHubSharp;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddAccountViewModel : BaseViewModel 
    {
        private GitHubAccount _attemptedAccount;
        private readonly IApplicationService _application;
        private readonly ILoginFactory _loginFactory;

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

        public ReactiveUI.IReactiveCommand<Unit> LoginCommand { get; }

        public AddAccountViewModel(IApplicationService application, ILoginFactory loginFactory)
        {
            _application = application;
            _loginFactory = loginFactory;
            LoginCommand = ReactiveUI.ReactiveCommand.CreateAsyncTask(_ => Login());
        }

        public void Init(NavObject navObject)
        {
            if (navObject.AttemptedAccountId >= 0)
                _attemptedAccount = this.GetApplication().Accounts.Find(navObject.AttemptedAccountId);

            if (_attemptedAccount != null)
            {
                Username = _attemptedAccount.Username;
                Domain = _attemptedAccount.Domain;
            }
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
                var account = await _loginFactory.LoginWithBasic(apiUrl, Username, Password, TwoFactor);
                var client = await _loginFactory.LoginAccount(account);
                _application.ActivateUser(account, client);
                ReactiveUI.MessageBus.Current.SendMessage(new LogoutMessage());
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

        public class NavObject
        {
            public int AttemptedAccountId { get; set; }

            public NavObject()
            {
                AttemptedAccountId = int.MinValue;
            }
        }
    }
}

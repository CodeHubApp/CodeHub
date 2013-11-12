using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddAccountViewModel : BaseViewModel 
    {
        private GitHubAccount _attemptedAccount;
        private readonly IApplicationService _application;
        private readonly ILoginService _loginService;
        private string _username;
        private string _password;
        private string _domain;
        private bool _isLoggingIn;

        public event EventHandler<Exception> LoginException;

        protected virtual void OnLoginException(Exception e)
        {
            var handler = LoginException;
            if (handler != null) handler(this, e);
        }

        public bool IsEnterprise { get; private set; }

        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            set { _isLoggingIn = value; RaisePropertyChanged(() => IsLoggingIn); }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; RaisePropertyChanged(() => Username); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; RaisePropertyChanged(() => Password); }
        }

        public string Domain
        {
            get { return _domain; }
            set { _domain = value; RaisePropertyChanged(() => Domain); }
        }

        public string TwoFactor { get; set; }

        public ICommand LoginCommand
        {
            get { return new MvxCommand(Login, CanLogin);}
        }

        public AddAccountViewModel(IApplicationService application, ILoginService loginService)
        {
            _application = application;
            _loginService = loginService;
        }

        public void Init(NavObject navObject)
        {
            _attemptedAccount = navObject.AttemptedAccount;

            if (_attemptedAccount != null)
            {
                Username = _attemptedAccount.Username;
                IsEnterprise = _attemptedAccount.Domain != null;
                if (IsEnterprise)
                    Domain = _attemptedAccount.Domain;
            }
            else
            {
                IsEnterprise = navObject.IsEnterprise;
            }
        }

        private bool CanLogin()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                return false;
            return true;
        }

        private async void Login()
        {
            var apiUrl = IsEnterprise ? Domain : null;
            if (apiUrl != null)
            {
                if (!apiUrl.StartsWith("http://") && !apiUrl.StartsWith("https://"))
                    apiUrl = "https://" + apiUrl;
                if (!apiUrl.EndsWith("/"))
                    apiUrl += "/";
                if (!apiUrl.Contains("/api/"))
                    apiUrl += "api/v3/";
            }

            // Get the accounts service so we can do some special things
            Exception exception = null;

            try
            {
                IsLoggingIn = true;
                var account = await Task.Run(() => _loginService.Authenticate(apiUrl, Username, Password, TwoFactor));
                var client = await Task.Run(() => _loginService.LoginAccount(account));
                _application.ActivateUser(account, client);
            }
            catch (Exception e)
            {
                TwoFactor = null;

                // Don't log an error for a two factor warning
                if (!(e is LoginService.TwoFactorRequiredException))
                {
                    Password = null;
                    ReportError("Error attempting to login via new User", e);
                }

                exception = e;
            }
            finally
            {
                IsLoggingIn = false;
            }

            if (exception != null)
                OnLoginException(exception);
        }

        public class NavObject
        {
            public bool IsEnterprise { get; set; }
            public GitHubAccount AttemptedAccount { get; set; }
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeFramework.Core.ViewModels;

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
			get { return new MvxCommand(() => Login(), CanLogin);}
        }

        public AddAccountViewModel(IApplicationService application, ILoginService loginService)
        {
            _application = application;
            _loginService = loginService;
        }

        public void Init(NavObject navObject)
        {
			if (navObject.AttemptedAccountId >= 0)
				_attemptedAccount = this.GetApplication().Accounts.Find(navObject.AttemptedAccountId) as GitHubAccount;

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

		private async Task Login()
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

            try
            {
                IsLoggingIn = true;
				Console.WriteLine(apiUrl);
				var loginData = await Task.Run(() => _loginService.Authenticate(apiUrl, Username, Password, TwoFactor, IsEnterprise, _attemptedAccount));
				var client = await Task.Run(() => _loginService.LoginAccount(loginData.Account));
				_application.ActivateUser(loginData.Account, client);
            }
            catch (Exception e)
            {
                TwoFactor = null;

                // Don't log an error for a two factor warning
                if (!(e is LoginService.TwoFactorRequiredException))
                {
                    Password = null;
					DisplayException(e);
                }
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        public class NavObject
        {
            public bool IsEnterprise { get; set; }
			public int AttemptedAccountId { get; set; }

			public NavObject()
			{
				AttemptedAccountId = int.MinValue;
			}
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Factories;
using MvvmCross.Core.ViewModels;
using ReactiveUI;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddAccountViewModel : BaseViewModel 
    {
        private GitHubAccount _attemptedAccount;
        private readonly IApplicationService _application;
        private readonly ILoginFactory _loginFactory;
        private string _username;
        private string _password;
        private string _domain;
        private bool _isLoggingIn;

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

        public AddAccountViewModel(IApplicationService application, ILoginFactory loginFactory)
        {
            _application = application;
            _loginFactory = loginFactory;
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

        private bool CanLogin()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                return false;
            return true;
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
                IsLoggingIn = true;
                var loginData = await _loginFactory.CreateLoginData(apiUrl, Username, Password, TwoFactor, true, _attemptedAccount);
				var client = await _loginFactory.LoginAccount(loginData.Account);
				_application.ActivateUser(loginData.Account, client);
                MessageBus.Current.SendMessage(new LogoutMessage());
            }
            catch (Exception e)
            {
                TwoFactor = null;

                // Don't log an error for a two factor warning
                if (!(e is LoginFactory.TwoFactorRequiredException))
                {
                    Password = null;
                    DisplayAlert(e.Message);
                }
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

using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using System;
using CodeHub.Core.Data;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels
{
    public abstract class BaseStartupViewModel : BaseViewModel
    {
        private bool _isLoggingIn;
        private string _status;
        private Uri _imageUrl;

        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            protected set
            {
                _isLoggingIn = value;
                RaisePropertyChanged(() => IsLoggingIn);
            }
        }

        public string Status
        {
            get { return _status; }
            protected set
            {
                _status = value;
                RaisePropertyChanged(() => Status);
            }
        }

        public Uri ImageUrl
        {
            get { return _imageUrl; }
            protected set
            {
                _imageUrl = value;
                RaisePropertyChanged(() => ImageUrl);
            }
        }

        public ICommand StartupCommand
        {
            get { return new MvxCommand(Startup);}
        }

        /// <summary>
        /// Execute startup code
        /// </summary>
        protected abstract void Startup();

        /// <summary>
        /// Gets the default account. If there is not one assigned it will pick the first in the account list.
        /// If there isn't one, it'll just return null.
        /// </summary>
        /// <returns>The default account.</returns>
        protected IAccount GetDefaultAccount()
        {
            var accounts = GetService<IAccountsService>();
            return accounts.GetDefault();
        }
    }
}

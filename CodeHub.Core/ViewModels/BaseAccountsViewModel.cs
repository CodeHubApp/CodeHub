using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.Data;
using CodeFramework.Core.Services;
using CodeFramework.Core.Utils;

namespace CodeFramework.Core.ViewModels
{
    public abstract class BaseAccountsViewModel : BaseViewModel
    {
        private readonly IAccountsService _accountsService;
        private readonly CustomObservableCollection<IAccount> _accounts = new CustomObservableCollection<IAccount>();
        private bool _isLoggingIn;

        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            protected set
            {
                _isLoggingIn = value;
                RaisePropertyChanged(() => IsLoggingIn);
            }
        }

        public CustomObservableCollection<IAccount> Accounts
        {
            get { return _accounts; }
        }

        public ICommand AddAccountCommand
        {
            get { return new MvxCommand(AddAccount); }
        }

        public ICommand SelectAccountCommand
        {
            get { return new MvxCommand<IAccount>(SelectAccount); }
        }

        protected BaseAccountsViewModel(IAccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        protected abstract void AddAccount();

        protected abstract void SelectAccount(IAccount account);

        public void Init()
        {
            _accounts.Reset(_accountsService);
        }
    }
}

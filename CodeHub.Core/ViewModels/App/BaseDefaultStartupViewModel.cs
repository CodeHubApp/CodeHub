using System.Linq;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;
using System;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
    public abstract class BaseDefaultStartupViewModel : BaseViewModel
    {
        private readonly IAccountsService _accountsService;
        private readonly Type _menuViewModelType;

        private readonly CollectionViewModel<string> _startupViews = new CollectionViewModel<string>();
        public CollectionViewModel<string> StartupViews
        {
            get { return _startupViews; }
        }

        private string _selectedStartupView;
        public string SelectedStartupView
        {
            get { return _selectedStartupView; }
            set
            {
                _selectedStartupView = value;
                RaisePropertyChanged(() => SelectedStartupView);
            }
        }

        protected BaseDefaultStartupViewModel(IAccountsService accountsService, Type menuViewModelType)
        {
            _accountsService = accountsService;
            _menuViewModelType = menuViewModelType;
        }

        public void Init()
        {
            var props = from p in _menuViewModelType.GetProperties()
                        let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true)
                        where attr.Length == 1
                        select attr[0] as PotentialStartupViewAttribute;

            SelectedStartupView = _accountsService.ActiveAccount.DefaultStartupView;
            StartupViews.Items.Reset(props.Select(x => x.Name));

            this.Bind(x => SelectedStartupView).Subscribe(x =>
            {
                _accountsService.ActiveAccount.DefaultStartupView = x;
                _accountsService.Update(_accountsService.ActiveAccount);
                ChangePresentation(new MvxClosePresentationHint(this));
            });
        }
    }
}


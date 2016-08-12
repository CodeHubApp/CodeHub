using CodeHub.Core.Services;
using System;
using System.Linq;
using CodeHub.Core.Utils;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
    public class DefaultStartupViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Type _menuViewModelType = typeof(MenuViewModel);

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

        public DefaultStartupViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        public void Init()
        {
            var props = from p in _menuViewModelType.GetProperties()
                        let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true)
                        where attr.Length == 1
                        select attr[0] as PotentialStartupViewAttribute;

            SelectedStartupView = _applicationService.Account.DefaultStartupView;
            StartupViews.Items.Reset(props.Select(x => x.Name));

            this.Bind(x => SelectedStartupView).Subscribe(x =>
            {
                _applicationService.Account.DefaultStartupView = x;
                _applicationService.UpdateActiveAccount().ToBackground();
                ChangePresentation(new MvxClosePresentationHint(this));
            });
        }
    }
}


using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reflection;
using System.Reactive.Linq;
using CodeHub.Core.Utilities;
using System.Linq;
using CodeHub.Core.ViewModels.App;
using CodeHub.Core.Data;

namespace CodeHub.Core.ViewModels.Settings
{
    public class DefaultStartupViewModel : BaseViewModel
    {
        public IReadOnlyReactiveList<string> StartupViews { get; private set; } 

        private string _selectedStartupView;
        public string SelectedStartupView
        {
            get { return _selectedStartupView; }
            set { this.RaiseAndSetIfChanged(ref _selectedStartupView, value); }
        }

        public DefaultStartupViewModel(ISessionService sessionService, IAccountsRepository accountsService)
        {
            Title = "Default Startup View";

            var menuViewModelType = typeof(MenuViewModel);

            SelectedStartupView = sessionService.Account.DefaultStartupView;
            this.WhenAnyValue(x => x.SelectedStartupView).Skip(1).Subscribe(x =>
            {
                sessionService.Account.DefaultStartupView = x;
                accountsService.Update(sessionService.Account);
                Dismiss();
            });

            StartupViews = new ReactiveList<string>(from p in menuViewModelType.GetRuntimeProperties()
                let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true).ToList()
                where attr.Count == 1 && attr[0] is PotentialStartupViewAttribute
                select ((PotentialStartupViewAttribute)attr[0]).Name);
        }
    }
}



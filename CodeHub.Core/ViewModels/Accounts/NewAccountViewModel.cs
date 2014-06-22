using System;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using CodeFramework.Core.ViewModels.Application;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class NewAccountViewModel : BaseViewModel, IAddAccountViewModel
    {
        public IReactiveCommand GoToEnterpriseLoginCommand { get; private set; }

        public IReactiveCommand GoToDotComLoginCommand { get; private set; }

        public NewAccountViewModel()
        {
            GoToDotComLoginCommand = new ReactiveCommand();
            GoToDotComLoginCommand.Subscribe(_ => 
                ShowViewModel(CreateViewModel<LoginViewModel>()));

            GoToEnterpriseLoginCommand = new ReactiveCommand();
            GoToEnterpriseLoginCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<AddAccountViewModel>();
                vm.IsEnterprise = true;
                ShowViewModel(vm);
            });

        }
    }
}

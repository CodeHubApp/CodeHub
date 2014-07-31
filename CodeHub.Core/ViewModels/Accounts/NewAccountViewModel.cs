using System;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using CodeFramework.Core.ViewModels.Application;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class NewAccountViewModel : BaseViewModel, IAddAccountViewModel
    {
        public IReactiveCommand<object> GoToEnterpriseLoginCommand { get; private set; }

        public IReactiveCommand<object> GoToDotComLoginCommand { get; private set; }

        public NewAccountViewModel()
        {
            GoToDotComLoginCommand = ReactiveCommand.Create();
            GoToDotComLoginCommand.Subscribe(_ => 
                CreateAndShowViewModel<LoginViewModel>());

            GoToEnterpriseLoginCommand = ReactiveCommand.Create();
            GoToEnterpriseLoginCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<AddAccountViewModel>();
                vm.IsEnterprise = true;
                ShowViewModel(vm);
            });

        }
    }
}

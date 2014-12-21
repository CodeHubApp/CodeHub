using System;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class NewAccountViewModel : BaseViewModel
    {
        public IReactiveCommand<object> GoToEnterpriseLoginCommand { get; private set; }

        public IReactiveCommand<object> GoToDotComLoginCommand { get; private set; }

        public NewAccountViewModel()
        {
            Title = "Account";

            GoToDotComLoginCommand = ReactiveCommand.Create()
                .WithSubscription(_ =>
                    NavigateTo(this.CreateViewModel<LoginViewModel>()));

            GoToEnterpriseLoginCommand = ReactiveCommand.Create()
                .WithSubscription(_ =>
                {
                    var vm = this.CreateViewModel<AddAccountViewModel>();
                    vm.IsEnterprise = true;
                    NavigateTo(vm);
                });
        }
    }
}

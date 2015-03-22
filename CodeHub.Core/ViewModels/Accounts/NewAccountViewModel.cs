using ReactiveUI;

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
                .WithSubscription(_ => NavigateTo(this.CreateViewModel<OAuthFlowLoginViewModel>()));

            GoToEnterpriseLoginCommand = ReactiveCommand.Create()
                .WithSubscription(_ => NavigateTo(this.CreateViewModel<AddEnterpriseAccountViewModel>()));
        }
    }
}

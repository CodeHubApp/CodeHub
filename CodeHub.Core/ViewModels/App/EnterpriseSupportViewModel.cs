using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.App
{
    public class EnterpriseSupportViewModel : BaseViewModel
    {
        public IReactiveCommand<object> SubmitFeedbackCommand { get; private set; }

        public IReactiveCommand<object> GoToGitHubCommand { get; private set; }

        public EnterpriseSupportViewModel()
        {
            Title = "Support";
            SubmitFeedbackCommand = ReactiveCommand.Create();

            GoToGitHubCommand = ReactiveCommand.Create();
            GoToGitHubCommand.Subscribe(_ => {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Init("https://github.com/thedillonb/CodeHub");
                NavigateTo(vm);
            });
        }
    }
}


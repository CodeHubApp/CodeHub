using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Users
{
    public abstract class BaseUserCollectionViewModel : BaseViewModel
    {
        public ReactiveList<BasicUserModel> Users { get; private set; }

        public IReactiveCommand<object> GoToUserCommand { get; private set; }

        protected BaseUserCollectionViewModel()
        {
            Users = new ReactiveList<BasicUserModel>();
            GoToUserCommand = ReactiveCommand.Create();
            GoToUserCommand.OfType<BasicUserModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<ProfileViewModel>();
                vm.Username = x.Login;
                ShowViewModel(vm);
            });
        }
    }
}

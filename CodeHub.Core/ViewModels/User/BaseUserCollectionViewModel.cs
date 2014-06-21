using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.User
{
    public abstract class BaseUserCollectionViewModel : LoadableViewModel
    {
        public ReactiveCollection<BasicUserModel> Users { get; private set; }

        public IReactiveCommand GoToUserCommand { get; private set; }

        protected BaseUserCollectionViewModel()
        {
            Users = new ReactiveCollection<BasicUserModel>();
            GoToUserCommand = new ReactiveCommand();
            GoToUserCommand.OfType<BasicUserModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<ProfileViewModel>();
                vm.Username = x.Login;
                ShowViewModel(vm);
            });
        }
    }
}

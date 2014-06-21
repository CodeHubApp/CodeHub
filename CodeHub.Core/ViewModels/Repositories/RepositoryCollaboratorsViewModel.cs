using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryCollaboratorsViewModel : LoadableViewModel
    {
        public ReactiveCollection<BasicUserModel> Collaborators { get; private set; }

        public string Username { get; set; }

        public string Repository { get; set; }

        public IReactiveCommand GoToUserCommand { get; private set; }

        public RepositoryCollaboratorsViewModel(IApplicationService applicationService)
        {
            Collaborators = new ReactiveCollection<BasicUserModel>();

            GoToUserCommand = new ReactiveCommand();
            GoToUserCommand.OfType<BasicUserModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<ProfileViewModel>();
                vm.Username = x.Login;
                ShowViewModel(vm);
            });

            LoadCommand.RegisterAsyncTask(t =>
                Collaborators.SimpleCollectionLoad(
                    applicationService.Client.Users[Username].Repositories[Repository].GetCollaborators(), t as bool?));
        }
    }
}


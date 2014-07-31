using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Teams
{
    public class TeamsViewModel : BaseViewModel, ILoadableViewModel
    {
        public ReactiveCollection<TeamShortModel> Teams { get; private set; }

        public string OrganizationName { get; set; }

        public IReactiveCommand<object> GoToTeamCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public TeamsViewModel(IApplicationService applicationService)
        {
            Teams = new ReactiveCollection<TeamShortModel>();

            GoToTeamCommand =  ReactiveCommand.Create();
            GoToTeamCommand.OfType<TeamShortModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<TeamMembersViewModel>();
                vm.Id = x.Id;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => 
                			Teams.SimpleCollectionLoad(applicationService.Client.Organizations[OrganizationName].GetTeams(), x as bool?));
        }
    }
}
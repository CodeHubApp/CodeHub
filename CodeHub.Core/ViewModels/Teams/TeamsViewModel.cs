using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.Core;

namespace CodeHub.Core.ViewModels.Teams
{
    public class TeamsViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        private readonly ReactiveList<TeamShortModel> _teams = new ReactiveList<TeamShortModel>();

        public IReadOnlyReactiveList<TeamShortModel> Teams { get; private set; }

        public string OrganizationName { get; set; }

        public IReactiveCommand<object> GoToTeamCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public TeamsViewModel(IApplicationService applicationService)
        {
            Title = "Teams";
            Teams = _teams.CreateDerivedCollection(x => x,
                x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToTeamCommand =  ReactiveCommand.Create();
            GoToTeamCommand.OfType<TeamShortModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<TeamMembersViewModel>();
                vm.Id = x.Id;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => 
                			_teams.SimpleCollectionLoad(applicationService.Client.Organizations[OrganizationName].GetTeams(), x as bool?));
        }
    }
}
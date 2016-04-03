using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationViewModel : LoadableViewModel
    {
        private UserModel _userModel;

        public string Name { get; private set; }

        public void Init(NavObject navObject) 
        {
            Name = navObject.Name;
        }

        public UserModel Organization
        {
            get { return _userModel; }
            private set { this.RaiseAndSetIfChanged(ref _userModel, value); }
        }

        public ICommand GoToMembersCommand
        {
            get { return new MvxCommand(() => ShowViewModel<OrganizationMembersViewModel>(new OrganizationMembersViewModel.NavObject { Name = Name }));}
        }

        public ICommand GoToTeamsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<TeamsViewModel>(new TeamsViewModel.NavObject { Name = Name })); }
        }

        public ICommand GoToFollowersCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserFollowersViewModel>(new UserFollowersViewModel.NavObject { Username = Name })); }
        }

        public ICommand GoToEventsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserEventsViewModel>(new UserEventsViewModel.NavObject { Username = Name })); }
        }

        public ICommand GoToGistsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserGistsViewModel>(new UserGistsViewModel.NavObject { Username = Name })); }
        }

        public ICommand GoToRepositoriesCommand
        {
            get { return new MvxCommand(() => ShowViewModel<OrganizationRepositoriesViewModel>(new OrganizationRepositoriesViewModel.NavObject { Name = Name })); }
        }

        protected override Task Load()
        {
            return this.RequestModel(this.GetApplication().Client.Organizations[Name].Get(), response => Organization = response.Data);
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
    }
}


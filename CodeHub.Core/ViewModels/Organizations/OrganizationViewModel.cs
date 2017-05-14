using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels.Events;
using GitHubSharp.Models;

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

        public ICommand GoToTeamsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<TeamsViewModel>(new TeamsViewModel.NavObject { Name = Name })); }
        }

        public ICommand GoToEventsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserEventsViewModel>(new UserEventsViewModel.NavObject { Username = Name })); }
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


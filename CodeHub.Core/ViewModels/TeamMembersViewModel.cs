using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class TeamMembersViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<BasicUserModel> _users = new CollectionViewModel<BasicUserModel>();

        public CollectionViewModel<BasicUserModel> Users
        {
            get
            {
                return _users;
            }
        }

        public ulong Id
        {
            get;
            private set;
        }

        public ICommand GoToUserCommand
        {
            get { return new MvxCommand<BasicUserModel>(x => this.ShowViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = x.Login })); }
        }

        public void Init(NavObject navObject)
        {
            Id = navObject.Id;
        }

        public Task Load(bool forceDataRefresh)
        {
            return Users.SimpleCollectionLoad(Application.Client.Teams[Id].GetMembers(), forceDataRefresh);
        }

        public class NavObject
        {
            public ulong Id { get; set; }
        }
    }
}
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels
{
    public class OrganizationMembersViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<BasicUserModel> _members = new CollectionViewModel<BasicUserModel>();

        public CollectionViewModel<BasicUserModel> Members
        {
            get { return _members; }
        }

        public ICommand GoToUserCommand
        {
            get { return new MvxCommand<BasicUserModel>(x => this.ShowViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = x.Login })); }
        }

        public string OrganizationName 
        { 
            get; 
            private set; 
        }

        public void Init(NavObject navObject)
        {
            OrganizationName = navObject.Name;
        }

        public class NavObject
        {
            public string Name { get; set; }
        }

        public Task Load(bool forceDataRefresh)
        {
            return Members.SimpleCollectionLoad(Application.Client.Organizations[OrganizationName].GetMembers(), forceDataRefresh);
        }
    }
}


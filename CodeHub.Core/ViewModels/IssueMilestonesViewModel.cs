using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class IssueMilestonesViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<MilestoneModel> _milestones = new CollectionViewModel<MilestoneModel>();

        public CollectionViewModel<MilestoneModel> Milestones
        {
            get { return _milestones; }
        }

        public string Username
        {
            get;
            private set;
        }

        public string Repository
        {
            get;
            private set;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
        }

        public Task Load(bool forceDataRefresh)
        {
            return Milestones.SimpleCollectionLoad(Application.Client.Users[Username].Repositories[Repository].Milestones.GetAll(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}


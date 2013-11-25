using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : LoadableViewModel
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

        protected override Task Load(bool forceCacheInvalidation)
        {
			return Milestones.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Milestones.GetAll(), forceCacheInvalidation);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}


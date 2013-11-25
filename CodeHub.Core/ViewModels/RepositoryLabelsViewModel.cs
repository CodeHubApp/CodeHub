using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class RepositoryLabelsViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<LabelModel> _labels = new CollectionViewModel<LabelModel>();

        public CollectionViewModel<LabelModel> Labels
        {
            get { return _labels; }
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

        protected override Task Load(bool forceDataRefresh)
        {
			return Labels.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetLabels(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}


using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class TagsViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<TagModel> _tags = new CollectionViewModel<TagModel>();

        public CollectionViewModel<TagModel> Tags
        {
            get { return _tags; }
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

        public ICommand GoToSourceCommand
        {
            get { return new MvxCommand<TagModel>(x => ShowViewModel<SourceViewModel>(new SourceViewModel.NavObject { Username = Username, Repository = Repository, Branch = x.Commit.Sha }));}
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
        }

        public Task Load(bool forceDataRefresh)
        {
            return Tags.SimpleCollectionLoad(Application.Client.Users[Username].Repositories[Repository].GetTags(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set;}
            public string Repository { get; set; }
        }
    }
}


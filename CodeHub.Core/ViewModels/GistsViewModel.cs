using System.Collections.Generic;
using CodeFramework.Core.ViewModels;
using GitHubSharp;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class UserGistsViewModel : GistsViewModel
    {
        public string Username
        {
            get;
            private set;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
            return Application.Client.Users[Username].Gists.GetGists();
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }

    public class StarredGistsViewModel : GistsViewModel
    {
        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
            return Application.Client.Gists.GetStarredGists();
        }
    }

    public class PublicGistsViewModel : GistsViewModel
    {
        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
            return Application.Client.Gists.GetPublicGists();
        }
    }


    public abstract class GistsViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<GistModel> _gists = new CollectionViewModel<GistModel>();

        public CollectionViewModel<GistModel> Gists
        {
            get
            {
                return _gists;
            }
        }
        
        public Task Load(bool forceDataRefresh)
        {
            return Gists.SimpleCollectionLoad(CreateRequest(), forceDataRefresh);
        }

        protected abstract GitHubRequest<List<GistModel>> CreateRequest();
    }
}


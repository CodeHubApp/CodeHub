using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class AccountGistsController : GistsController
    {
        private readonly string _username;

        public AccountGistsController(IListView<GistModel> view, string username)
            : base(view)
        {
            _username = username;
        }


        protected override GitHubRequest<List<GistModel>> GetData()
        {
            return Application.Client.Users[_username].Gists.GetGists();
        }
    }

    public class StarredGistsController : GistsController
    {
        public StarredGistsController(IListView<GistModel> view)
            : base(view)
        {
        }

        protected override GitHubRequest<List<GistModel>> GetData()
        {
            return Application.Client.Gists.GetStarredGists();
        }
    }

    public class PublicGistsController : GistsController
    {
        public PublicGistsController(IListView<GistModel> view)
            : base(view)
        {
        }

        protected override GitHubRequest<List<GistModel>> GetData()
        {
            return Application.Client.Gists.GetPublicGists();
        }
    }


    public abstract class GistsController : ListController<GistModel>
    {
        protected GistsController(IListView<GistModel> view)
            : base(view)
        {
        }

        protected abstract GitHubRequest<List<GistModel>> GetData();

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(GetData(), forceDataRefresh, response => {
                Model = new ListModel<GistModel>(response.Data, this.CreateMore(response));
                Refresh();
            });
        }
    }
}


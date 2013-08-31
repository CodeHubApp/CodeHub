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


        protected override GitHubResponse<List<GistModel>> GetData(bool force)
        {
            return Application.Client.Users[_username].Gists.GetGists(force);
        }
    }

    public class StarredGistsController : GistsController
    {
        public StarredGistsController(IListView<GistModel> view)
            : base(view)
        {
        }

        protected override GitHubResponse<List<GistModel>> GetData(bool force)
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

        protected override GitHubResponse<List<GistModel>> GetData(bool force)
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

        protected abstract GitHubResponse<List<GistModel>> GetData(bool force);

        public override void Update(bool force)
        {
            var response = GetData(force);
            Model = new ListModel<GistModel> { Data = response.Data, More = this.CreateMore(response) };
        }
    }
}


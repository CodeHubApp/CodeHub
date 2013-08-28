using System;
using CodeHub.Controllers;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using CodeHub.Data;
using CodeFramework.Controllers;
using CodeFramework.Elements;

namespace CodeHub.GitHub.Controllers.Gists
{
    public class AccountGistsController : GistsController
    {
        private string _username;

        public AccountGistsController(IListView<GistModel> view, string username)
            : base(view)
        {
            _username = username;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_username].Gists.GetGists(force);
            Model = new ListModel<GistModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }

    public class StarredGistsController : GistsController
    {
        public StarredGistsController(IListView<GistModel> view)
            : base(view)
        {
        }

        public override void Update(bool force)
        {
            Model = new ListModel<GistModel> {
                Data = Application.Client.Gists.GetStarredGists().Data
            };
        }
    }

    public class PublicGistsController : GistsController
    {
        public PublicGistsController(IListView<GistModel> view)
            : base(view)
        {
        }

        public override void Update(bool force)
        {
            Model = new ListModel<GistModel> {
                Data = Application.Client.Gists.GetPublicGists().Data
            };
        }
    }


    public abstract class GistsController : ListController<GistModel>
    {
        public GistsController(IListView<GistModel> view)
            : base(view)
        {
        }
    }
}


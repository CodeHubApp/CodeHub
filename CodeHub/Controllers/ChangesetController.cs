using System;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Linq;
using MonoTouch;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using GitHubSharp;


namespace CodeHub.Controllers
{
    public class ChangesetController : ListController<CommitModel>
    {
        private const int RequestLimit = 30;

        public string User { get; private set; }

        public string Slug { get; private set; }

        public ChangesetController(IListView<CommitModel> view, string user, string slug)
            : base(view)
        {
            User = user;
            Slug = slug;
        }

        public override void Update(bool force)
        {
            var response = GetData();
            Model = new ListModel<CommitModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }

        protected GitHubResponse<List<CommitModel>> GetData(string startNode = null)
        {
            return Application.Client.Users[User].Repositories[Slug].Commits.GetAll(startNode);
        }
    }
}


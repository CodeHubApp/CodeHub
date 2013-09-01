using System;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.Dialog;
using System.Linq;
using CodeFramework.Controllers;
using CodeHub.Filters.Models;

namespace CodeHub.Controllers
{
    public class IssuesController : BaseIssuesController
    {
        public string User { get; private set; }
        public string Slug { get; private set; }

        public IssuesController(IListView<IssueModel> view, string user, string slug)
            : base(view)
        {
            User = user;
            Slug = slug;
        }

        public override void Update(bool force)
        {
            var data = Application.Client.Users[User].Repositories[Slug].Issues.GetAll(force);
            Model = new ListModel<IssueModel> {Data = data.Data, More = this.CreateMore(data)};
        }
    }
}


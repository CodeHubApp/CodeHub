using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.Controllers
{
    public class BranchesController : ListController<BranchModel>
    {
        private readonly string _username;
        private readonly string _slug;

        public BranchesController(IView<ListModel<BranchModel>> view, string username, string slug)
            : base(view)
        {
            _username = username;
            _slug = slug;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_username].Repositories[_slug].GetBranches();
            Model = new ListModel<BranchModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }
}


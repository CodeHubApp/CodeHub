using System;
using GitHubSharp.Models;
using CodeFramework.Controllers;
using System.Collections.Generic;

namespace CodeHub.Controllers
{
    public class RepositoriesExploreController : ListController<RepositorySearchModel.RepositoryModel>
    {
        public bool Searched { get; private set; }

        public RepositoriesExploreController(IListView<RepositorySearchModel.RepositoryModel> view)
            : base(view)
        {
        }

        public override void Update(bool force)
        {
            //Don't do anything here...
            Model = new ListModel<RepositorySearchModel.RepositoryModel>() { Data = new List<RepositorySearchModel.RepositoryModel>() };
        }

        public void Search(string text)
        {
            Searched = true;
			var response = Application.Client.Repositories.SearchRepositories(text);
			Model = new ListModel<RepositorySearchModel.RepositoryModel> { Data = response.Data.Repositories };
            Render();
        }

    }
}


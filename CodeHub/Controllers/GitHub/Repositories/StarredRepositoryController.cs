using System;
using System.Collections.Generic;
using GitHubSharp.Models;

namespace CodeHub.GitHub.Controllers.Repositories
{
    public class StarredRepositoryController : RepositoryController
    {
        public StarredRepositoryController()
            : base(string.Empty)
        {
            ShowOwner = true;
            Title = "Starred";
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var data = Application.Client.API.GetRepositoriesStarred();
            nextPage = data.Next == null ? -1 : currentPage + 1;
            return data.Data;
        }
    }
}


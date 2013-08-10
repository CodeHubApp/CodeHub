using System;
using System.Collections.Generic;
using GitHubSharp.Models;

namespace CodeHub.GitHub.Controllers.Repositories
{
    public class WatchedRepositoryController : RepositoryController
    {
        public WatchedRepositoryController(string username)
            : base(username)
        {
            ShowOwner = true;
            Title = "Watched";
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var data = Application.Client.API.GetRepositoriesWatching(Username);
            nextPage = data.Next == null ? -1 : currentPage + 1;
            return data.Data;
        }
    }
}


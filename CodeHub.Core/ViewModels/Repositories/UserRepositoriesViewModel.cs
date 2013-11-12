using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.User
{
    class UserRepositoriesViewModel : RepositoriesViewModel
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

        protected override Task Load(bool forceDataRefresh)
        {
            GitHubRequest<List<RepositoryModel>> request;
            if (string.Equals(Application.Account.Username, Username, StringComparison.OrdinalIgnoreCase))
                request = Application.Client.AuthenticatedUser.Repositories.GetAll();
            else
                request = Application.Client.Users[Username].Repositories.GetAll();
            return Repositories.SimpleCollectionLoad(request, forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

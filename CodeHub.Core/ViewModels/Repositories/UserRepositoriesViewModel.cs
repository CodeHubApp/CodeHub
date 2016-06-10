using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class UserRepositoriesViewModel : RepositoriesViewModel
    {
        public string Username { get; private set; }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        protected override Task Load()
        {
            GitHubRequest<List<RepositoryModel>> request;
            if (string.Equals(this.GetApplication().Account.Username, Username, StringComparison.OrdinalIgnoreCase))
                request = this.GetApplication().Client.AuthenticatedUser.Repositories.GetAll("owner");
            else
                request = this.GetApplication().Client.Users[Username].Repositories.GetAll();
            return Repositories.SimpleCollectionLoad(request);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

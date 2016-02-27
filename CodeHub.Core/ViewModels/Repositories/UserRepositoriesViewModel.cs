using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.User
{
    public class UserRepositoriesViewModel : RepositoriesViewModel
    {
        public string Username { get; private set; }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        protected override Task Load(bool forceCacheInvalidation)
        {
            GitHubRequest<List<RepositoryModel>> request;
            if (string.Equals(this.GetApplication().Account.Username, Username, StringComparison.OrdinalIgnoreCase))
                request = this.GetApplication().Client.AuthenticatedUser.Repositories.GetAll();
            else
                request = this.GetApplication().Client.Users[Username].Repositories.GetAll();
            return Repositories.SimpleCollectionLoad(request, forceCacheInvalidation);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

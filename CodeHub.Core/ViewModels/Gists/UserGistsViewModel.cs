using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Gists
{
    public class UserGistsViewModel : GistsViewModel
    {
        public string Username
        {
            get;
            private set;
        }

        public UserGistsViewModel(IApplicationService application)
            : base(application)
        {
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
            return Application.Client.Users[Username].Gists.GetGists();
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }

}

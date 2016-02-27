using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public class OrganizationEventsViewModel : BaseEventsViewModel
    {
        public string OrganizationName
        {
            get;
            private set;
        }

        public string Username
        {
            get;
            private set;
        }

        public void Init(NavObject navobject)
        {
            Username = navobject.Username;
            OrganizationName = navobject.OrganizagtionName;
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
            return this.GetApplication().Client.Users[Username].GetOrganizationEvents(OrganizationName, page, perPage);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string OrganizagtionName { get; set; }
        }
    }

}

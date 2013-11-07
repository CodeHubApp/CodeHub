using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public class UserEventsViewModel : EventsViewModel
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

        protected override GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
            return Application.Client.Users[Username].GetEvents(page, perPage);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

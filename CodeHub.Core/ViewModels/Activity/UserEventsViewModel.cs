using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Activity
{
    public class UserEventsViewModel : BaseEventsViewModel
    {
        public string Username { get; private set; }

        public UserEventsViewModel(ISessionService sessionService)
            : base(sessionService)
        {
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest()
        {
			return SessionService.Client.Users[Username].GetEvents();
        }

        public UserEventsViewModel Init(string username)
        {
            Username = username;
            return this;
        }
    }
}

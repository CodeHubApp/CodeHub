using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Activity
{
    public class NewsViewModel : BaseEventsViewModel
    {
        public NewsViewModel(ISessionService sessionService) 
            : base(sessionService)
        {
            Title = "News";
        }

        protected override GitHubSharp.GitHubRequest<List<EventModel>> CreateRequest()
        {
			return SessionService.Client.Users[SessionService.Account.Username].GetReceivedEvents();
        }
    }
}


using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Activity
{
    public class NewsViewModel : BaseEventsViewModel
    {
        public NewsViewModel(ISessionService applicationService) 
            : base(applicationService)
        {
            Title = "News";
        }

        protected override GitHubSharp.GitHubRequest<List<EventModel>> CreateRequest()
        {
			return ApplicationService.Client.Users[ApplicationService.Account.Username].GetReceivedEvents();
        }
    }
}


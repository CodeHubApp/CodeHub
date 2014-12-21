using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public class NewsViewModel : BaseEventsViewModel
    {
        public NewsViewModel(IApplicationService applicationService) 
            : base(applicationService)
        {
            Title = "News";
        }

        protected override GitHubSharp.GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
			return ApplicationService.Client.Users[ApplicationService.Account.Username].GetReceivedEvents(page, perPage);
        }
    }
}


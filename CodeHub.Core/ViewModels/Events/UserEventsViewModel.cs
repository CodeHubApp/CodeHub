using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public class UserEventsViewModel : BaseEventsViewModel
    {
        public string Username { get; set; }

        public UserEventsViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
			return ApplicationService.Client.Users[Username].GetEvents(page, perPage);
        }
    }
}

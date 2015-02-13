using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Activity
{
    public class UserEventsViewModel : BaseEventsViewModel
    {
        public string Username { get; set; }

        public UserEventsViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest()
        {
			return ApplicationService.Client.Users[Username].GetEvents();
        }
    }
}

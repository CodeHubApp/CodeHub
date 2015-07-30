using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Activity
{
    public class OrganizationEventsViewModel : BaseEventsViewModel
    {
        public string OrganizationName { get; set; }

        public string Username { get; set; }

        public OrganizationEventsViewModel(ISessionService applicationService)
            : base(applicationService)
        {
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest()
        {
			return SessionService.Client.Users[Username].GetOrganizationEvents(OrganizationName);
        }
    }
}

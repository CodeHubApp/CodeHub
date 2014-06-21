using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public class OrganizationEventsViewModel : BaseEventsViewModel
    {
        public string OrganizationName { get; set; }

        public string Username { get; set; }

        public OrganizationEventsViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
			return ApplicationService.Client.Users[Username].GetOrganizationEvents(OrganizationName, page, perPage);
        }
    }
}

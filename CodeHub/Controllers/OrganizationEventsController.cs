using System;
using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class OrganizationEventsController : ListController<EventModel>
    {
        public string Name { get; private set; }

        public string User { get; private set; }

        public OrganizationEventsController(IListView<EventModel> view, string user, string name)
            : base(view)
        {
            User = user;
            Name = name;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[User].GetOrganizationEvents(Name), forceDataRefresh, response => {
                RenderView(new ListModel<EventModel>(EventsController.ExpandConsolidatedEvents(response.Data), this.CreateMore(response, EventsController.ExpandConsolidatedEvents)));
            });
        }
    }
}


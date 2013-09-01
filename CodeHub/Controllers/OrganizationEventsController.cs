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

        public override void Update(bool force)
        {
            var response = Application.Client.Users[User].GetOrganizationEvents(Name, force);
            Model = new ListModel<EventModel> { Data = EventsController.ExpandConsolidatedEvents(response.Data) };
            Model.More = this.CreateMore(response, EventsController.ExpandConsolidatedEvents);
        }
    }
}


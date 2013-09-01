using GitHubSharp.Models;
using System.Collections.Generic;
using CodeFramework.Controllers;
using GitHubSharp;

namespace CodeHub.Controllers
{
    public class RepositoryEventsController : ListController<EventModel>
    {
        public string User { get; private set; }

        public string Slug { get; private set; }

        public RepositoryEventsController(IListView<EventModel> view, string user, string slug)
            : base(view)
        {
            User = user;
            Slug = slug;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[User].Repositories[Slug].GetEvents(force);
            Model = new ListModel<EventModel> {Data = EventsController.ExpandConsolidatedEvents(response.Data), 
                More = this.CreateMore(response, EventsController.ExpandConsolidatedEvents)};
        }
    }
}
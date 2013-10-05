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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[User].Repositories[Slug].GetEvents(), forceDataRefresh, response => {
                RenderView(new ListModel<EventModel>(EventsController.ExpandConsolidatedEvents(response.Data), this.CreateMore(response, EventsController.ExpandConsolidatedEvents)));
            });
        }
    }
}
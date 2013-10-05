using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class NewsController : ListController<EventModel>
    {
        public NewsController(IListView<EventModel> view)
            : base(view)
        {
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[Application.Account.Username].GetReceivedEvents(), forceDataRefresh, response => {
                RenderView(new ListModel<EventModel>(EventsController.ExpandConsolidatedEvents(response.Data), 
                                                     this.CreateMore(response, EventsController.ExpandConsolidatedEvents)));
            });
        }
    }
}


using System;
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

        public override void Update(bool force)
        {
            var response = Application.Client.Users[Application.Account.Username].GetReceivedEvents(force);
            Model = new ListModel<EventModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }
}


using System.Collections.Generic;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public class NewsViewModel : BaseEventsViewModel
    {
        protected override GitHubSharp.GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
            return Application.Client.Users[Application.Account.Username].GetReceivedEvents(page, perPage);
        }
    }
}


using GitHubSharp.Models;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels
{
    public class NewsViewModel : EventsViewModel
    {
        protected override GitHubSharp.GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
            return Application.Client.Users[Application.Account.Username].GetReceivedEvents(page, perPage);
        }
    }
}


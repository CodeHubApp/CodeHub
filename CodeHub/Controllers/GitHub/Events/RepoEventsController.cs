using System.Collections.Generic;
using GitHubSharp.Models;

namespace CodeHub.GitHub.Controllers.Events
{
    public class RepoEventsController : EventsController
    {
        public string Slug { get; private set; }
        
        public RepoEventsController(string username, string slug)
            : base(username)
        {
            Slug = slug;
        }
        
        protected override List<EventModel> OnGetData(int page = 1)
        {
            var response = Application.Client.API.GetRepositoryEvents(Username, Slug, page);
            _nextPage = response.Next != null ? page + 1 : -1;
            return response.Data;
        }
    }
}

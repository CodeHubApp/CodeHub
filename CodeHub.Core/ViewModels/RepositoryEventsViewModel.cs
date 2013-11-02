using GitHubSharp.Models;
using System.Collections.Generic;
using GitHubSharp;

namespace CodeHub.Core.ViewModels
{
    public class RepositoryEventsViewModel : EventsViewModel
    {
        public string Repository 
        { 
            get; 
            private set; 
        }

        public string Username
        {
            get;
            private set;
        }

        public RepositoryEventsViewModel(string username, string repository)
        {
            Username = username;
            Repository = repository;
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
            return Application.Client.Users[Username].Repositories[Repository].GetEvents(page, perPage);
        }
    }
}
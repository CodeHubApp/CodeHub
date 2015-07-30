using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Activity
{
    public class RepositoryEventsViewModel : BaseEventsViewModel
    {
        public string RepositoryName { get; private set; }

        public string RepositoryOwner { get; private set; }

        public RepositoryEventsViewModel(ISessionService applicationService)
            : base(applicationService)
        {
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest()
        {
			return SessionService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetEvents();
        }

        public RepositoryEventsViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}
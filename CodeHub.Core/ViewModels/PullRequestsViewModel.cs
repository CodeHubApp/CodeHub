using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;
using System.Threading.Tasks;
using System;

namespace CodeHub.Core.ViewModels
{
    public class PullRequestsViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly FilterableCollectionViewModel<PullRequestModel, PullRequestsFilterModel> _pullrequests;
        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            protected set
            {
                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
        }

        public FilterableCollectionViewModel<PullRequestModel, PullRequestsFilterModel> PullRequests
        {
            get { return _pullrequests; }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public PullRequestsViewModel(string username, string repository) 
        {
            Username = username;
            Repository = repository;

            _pullrequests = new FilterableCollectionViewModel<PullRequestModel, PullRequestsFilterModel>("PullRequests");
            _pullrequests.Bind(x => x.Filter, async () => {
                try
                {
                    IsLoading = true;
                    await Load(true);
                }
                catch (Exception e)
                {
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        public Task Load(bool forceDataRefresh)
        {
            var state = PullRequests.Filter.IsOpen ? "open" : "closed";
            var request = Application.Client.Users[Username].Repositories[Repository].PullRequests.GetAll(state: state);
            return PullRequests.SimpleCollectionLoad(request, forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

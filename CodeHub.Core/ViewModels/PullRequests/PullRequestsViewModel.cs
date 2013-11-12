using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel : LoadableViewModel
    {
        private readonly FilterableCollectionViewModel<PullRequestModel, PullRequestsFilterModel> _pullrequests;

        public FilterableCollectionViewModel<PullRequestModel, PullRequestsFilterModel> PullRequests
        {
            get { return _pullrequests; }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public ICommand GoToPullRequestCommand
        {
            get { return new MvxCommand<PullRequestModel>(x => ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = Username, Repository = Repository, Id = x.Number })); }
        }

        public PullRequestsViewModel(string username, string repository) 
        {
            Username = username;
            Repository = repository;

            _pullrequests = new FilterableCollectionViewModel<PullRequestModel, PullRequestsFilterModel>("PullRequests");
            _pullrequests.Bind(x => x.Filter, () => LoadCommand.Execute(true));
        }

        protected override Task Load(bool forceDataRefresh)
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

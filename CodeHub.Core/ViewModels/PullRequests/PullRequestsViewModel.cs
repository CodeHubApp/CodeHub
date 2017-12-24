using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Messages;
using System.Linq;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;
        private readonly IApplicationService _applicationService;
        private IDisposable _pullRequestEditSubscription;

        private readonly CollectionViewModel<Octokit.PullRequest> _pullrequests = new CollectionViewModel<Octokit.PullRequest>();
        public CollectionViewModel<Octokit.PullRequest> PullRequests
        {
            get { return _pullrequests; }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set 
            {
                _selectedFilter = value;
                RaisePropertyChanged(() => SelectedFilter);
            }
        }

        public ICommand GoToPullRequestCommand
        {
            get { return new MvxCommand<Octokit.PullRequest>(x => ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = Username, Repository = Repository, Id = x.Number })); }
        }

        public PullRequestsViewModel(IMessageService messageService, IApplicationService applicationService)
        {
            _messageService = messageService;
            _applicationService = applicationService;
            this.Bind(x => x.SelectedFilter).Subscribe(_ => LoadCommand.Execute(null));
        }

        public void Init(NavObject navObject) 
        {
            Username = navObject.Username;
            Repository = navObject.Repository;

            PullRequests.FilteringFunction = x => {
                var state = SelectedFilter == 0 ? "open" : "closed";
                return x.Where(y => y.State == state);
            };

            _pullRequestEditSubscription = _messageService.Listen<PullRequestEditMessage>(x =>
            {
                if (x.PullRequest == null)
                    return;
          
                var index = PullRequests.Items.IndexOf(x.PullRequest);
                if (index < 0)
                    return;
                PullRequests.Items[index] = x.PullRequest;
                PullRequests.Refresh();
            });
        }

        protected override async Task Load()
        {
            var request = new Octokit.PullRequestRequest
            {
                State = SelectedFilter == 0 ? Octokit.ItemStateFilter.Open : Octokit.ItemStateFilter.Closed
            };

            var pullRequests = await _applicationService.GitHubClient.PullRequest.GetAllForRepository(Username, Repository, request);
            PullRequests.Items.Reset(pullRequests);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

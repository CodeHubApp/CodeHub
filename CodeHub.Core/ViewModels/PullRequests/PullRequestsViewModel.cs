using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;
        private IDisposable _pullRequestEditSubscription;

        public ReactiveList<Octokit.PullRequest> PullRequests { get; } = new ReactiveList<Octokit.PullRequest>();

        public string Username { get; }

        public string Repository { get; }

        private int _selectedFilter;
        public int SelectedFilter
        {
            get => _selectedFilter;
            set => this.RaiseAndSetIfChanged(ref _selectedFilter, value);
        }

        //public ICommand GoToPullRequestCommand
        //{
        //    get { return new MvxCommand<PullRequestModel>(x => ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = Username, Repository = Repository, Id = x.Number })); }
        //}

        public PullRequestsViewModel(
            string username,
            string repository,
            IMessageService messageService = null)
        {
            Username = username;
            Repository = repository;

            _messageService = messageService ?? Locator.Current.GetService<IMessageService>();

            this.WhenAnyValue(x => x.SelectedFilter)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(LoadCommand);

            _pullRequestEditSubscription = _messageService.Listen<PullRequestEditMessage>(x =>
            {
                if (x.PullRequest == null)
                    return;

                var index = PullRequests.IndexOf(x.PullRequest);
                if (index < 0)
                    return;
                
                PullRequests[index] = x.PullRequest;
            });
        }

        protected override async Task Load()
        {
            var request = new Octokit.PullRequestRequest
            {
                State = SelectedFilter == 0 ? Octokit.ItemStateFilter.Open : Octokit.ItemStateFilter.Closed
            };

            var result = await this.GetApplication().GitHubClient.PullRequest.GetAllForRepository(Username, Repository, request);
            PullRequests.Reset(result);
        }
    }
}

using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel : BaseSearchableListViewModel<PullRequest, PullRequestItemViewModel>
    {
        private readonly ISessionService _sessionService;

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
        }

        public PullRequestsViewModel(ISessionService sessionService)
		{
            _sessionService = sessionService;
            Title = "Pull Requests";

            Items = InternalItems.CreateDerivedCollection(x => {
                    var vm = new PullRequestItemViewModel(x);
                    vm.GoToCommand.Subscribe(_ => {
                        var prViewModel = this.CreateViewModel<PullRequestViewModel>();
                        prViewModel.Init(RepositoryOwner, RepositoryName, x.Number, x);
                        NavigateTo(prViewModel);

                        prViewModel.WhenAnyValue(y => y.Issue.State)
                            .DistinctUntilChanged()
                            .Skip(1)
                            .Subscribe(y => LoadCommand.ExecuteIfCan());
                    });
                    return vm;
                },
                filter: x => x.Title.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                InternalItems.Reset(await RetrievePullRequests());
            });

            this.WhenAnyValue(x => x.SelectedFilter).Skip(1).Subscribe(_ => {
                InternalItems.Clear();
                LoadCommand.ExecuteIfCan();
            });
		}

        private async Task<IReadOnlyList<PullRequest>> RetrievePullRequests(int page = 1)
        {
            var connection = _sessionService.GitHubClient.Connection;
            var parameters = new Dictionary<string, string>();
            parameters["page"] = page.ToString();
            parameters["per_page"] = 50.ToString();
            parameters["state"] = SelectedFilter == 0 ? "open" : "closed";
            var ret = await connection.Get<IReadOnlyList<PullRequest>>(ApiUrls.PullRequests(RepositoryOwner, RepositoryName), parameters, "application/json");

            if (ret.HttpResponse.ApiInfo.Links.ContainsKey("next"))
            {
                LoadMoreCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                    InternalItems.AddRange(await RetrievePullRequests(page + 1));
                });
            }
            else
            {
                LoadMoreCommand = null;
            }

            return ret.Body;
        }
    }
}

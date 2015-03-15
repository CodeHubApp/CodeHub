using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel : BaseViewModel, IPaginatableViewModel
    {
        public IReadOnlyReactiveList<PullRequestItemViewModel> PullRequests { get; private set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<Unit> LoadMoreCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public PullRequestsViewModel(ISessionService applicationService)
		{
            Title = "Pull Requests";

            var pullRequests = new ReactiveList<PullRequestModel>();
            PullRequests = pullRequests.CreateDerivedCollection(
                x => new PullRequestItemViewModel(x, () => {
                    var vm = this.CreateViewModel<PullRequestViewModel>();
                    vm.Init(RepositoryOwner, RepositoryName, (int)x.Number);
                    NavigateTo(vm);

                    vm.WhenAnyValue(y => y.Issue.State)
                        .DistinctUntilChanged()
                        .Skip(1)
                        .Subscribe(y => LoadCommand.ExecuteIfCan());
                }),
                filter: x => x.Title.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var state = SelectedFilter == 0 ? "open" : "closed";
			    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].PullRequests.GetAll(state: state);
                return pullRequests.SimpleCollectionLoad(request, 
                    x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x()));
            });

            this.WhenAnyValue(x => x.SelectedFilter).Skip(1).Subscribe(_ => 
            {
                pullRequests.Clear();
                LoadCommand.ExecuteIfCan();
            });
		}
    }
}

using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel : BaseViewModel, ILoadableViewModel
    {
        public ReactiveList<PullRequestModel> PullRequests { get; private set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
        }

        public IReactiveCommand<object> GoToPullRequestCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public PullRequestsViewModel(IApplicationService applicationService)
		{
            PullRequests = new ReactiveList<PullRequestModel>();

            GoToPullRequestCommand = ReactiveCommand.Create();
		    GoToPullRequestCommand.OfType<PullRequestModel>().Subscribe(pullRequest =>
		    {
		        var vm = CreateViewModel<PullRequestViewModel>();
		        vm.RepositoryOwner = RepositoryOwner;
		        vm.RepositoryName = RepositoryName;
		        vm.PullRequestId = pullRequest.Number;
		        vm.PullRequest = pullRequest;
		        vm.WhenAnyValue(x => x.PullRequest).Skip(1).Subscribe(x =>
		        {
                    var index = PullRequests.IndexOf(pullRequest);
                    if (index < 0) return;
                    PullRequests[index] = x;
                    PullRequests.Reset();
		        });
                ShowViewModel(vm);
		    });

		    this.WhenAnyValue(x => x.SelectedFilter).Skip(1).Subscribe(_ => LoadCommand.ExecuteIfCan());

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var state = SelectedFilter == 0 ? "open" : "closed";
			    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].PullRequests.GetAll(state: state);
                return PullRequests.SimpleCollectionLoad(request, t as bool?);
            });
		}
    }
}

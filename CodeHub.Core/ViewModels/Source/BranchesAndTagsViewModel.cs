using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Source
{
    public class BranchesAndTagsViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
	{
		private ShowIndex _selectedFilter;
        public ShowIndex SelectedFilter
		{
			get { return _selectedFilter; }
			set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
		}

		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public IReadOnlyReactiveList<BranchItemViewModel> Branches { get; private set; }

        public IReadOnlyReactiveList<TagItemViewModel> Tags { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

		public BranchesAndTagsViewModel(IApplicationService applicationService)
		{
            var branches = new ReactiveList<Octokit.Branch>();
            Branches = branches.CreateDerivedCollection(
                x => new BranchItemViewModel(x.Name, () =>
                {
                    var vm = this.CreateViewModel<SourceTreeViewModel>();
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    vm.Branch = x.Name;
                    vm.TrueBranch = true;
                    NavigateTo(vm);
                }), 
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            var tags = new ReactiveList<Octokit.RepositoryTag>();
            Tags = tags.CreateDerivedCollection(
                x => new TagItemViewModel(x.Name, () =>
                {
                    var vm = this.CreateViewModel<SourceTreeViewModel>();
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    vm.Branch = x.Commit.Sha;
                    NavigateTo(vm);
                }), 
                filter: x => x.Name.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
		    {
                if (SelectedFilter == ShowIndex.Branches)
                {
                    branches.Reset(await applicationService.GitHubClient.Repository.GetAllBranches(RepositoryOwner, RepositoryName));
                }
                else
                {
                    tags.Reset(await applicationService.GitHubClient.Repository.GetAllTags(RepositoryOwner, RepositoryName));
                }
		    });

            this.WhenAnyValue(x => x.SelectedFilter).Skip(1).Subscribe(_ => LoadCommand.ExecuteIfCan());
		}

	    public enum ShowIndex
	    {
	        Branches = 0,
            Tags = 1
	    }
	}
}


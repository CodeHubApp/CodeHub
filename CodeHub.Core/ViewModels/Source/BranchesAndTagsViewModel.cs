using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.Core;

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

        public IReadOnlyReactiveList<BranchModel> Branches { get; private set; }

        public IReadOnlyReactiveList<TagModel> Tags { get; private set; }

        public IReactiveCommand<object> GoToSourceCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

		public BranchesAndTagsViewModel(IApplicationService applicationService)
		{
            var branches = new ReactiveList<BranchModel>();
            Branches = branches.CreateDerivedCollection(x => x, 
                x => x.Name.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            var tags = new ReactiveList<TagModel>();
            Tags = tags.CreateDerivedCollection(x => x, 
                x => x.Name.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToSourceCommand = ReactiveCommand.Create();
		    GoToSourceCommand.OfType<BranchModel>().Subscribe(x =>
		    {
		        var vm = CreateViewModel<SourceTreeViewModel>();
		        vm.RepositoryOwner = RepositoryOwner;
		        vm.RepositoryName = RepositoryName;
		        vm.Branch = x.Name;
		        vm.TrueBranch = true;
                ShowViewModel(vm);
		    });
            GoToSourceCommand.OfType<TagModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<SourceTreeViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Branch = x.Commit.Sha;
                ShowViewModel(vm);
            });


		    this.WhenAnyValue(x => x.SelectedFilter).Skip(1).Subscribe(_ => LoadCommand.ExecuteIfCan());

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
		    {
                if (SelectedFilter == ShowIndex.Branches)
                {
                    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetBranches();
                    return branches.LoadAll<BranchModel>(request);
                }
                else
                {
                    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetTags();
                    return tags.LoadAll<TagModel>(request);
                }
		    });
		}

	    public enum ShowIndex
	    {
	        Branches = 0,
            Tags = 1
	    }
	}
}


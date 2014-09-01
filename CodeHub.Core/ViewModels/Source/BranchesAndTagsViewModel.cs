using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class BranchesAndTagsViewModel : BaseViewModel, ILoadableViewModel
	{
		private ShowIndex _selectedFilter;
        public ShowIndex SelectedFilter
		{
			get { return _selectedFilter; }
			set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
		}

		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public IReadOnlyReactiveList<object> Items { get; private set; }

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
            var items = new ReactiveList<object>();
            Items = items.CreateDerivedCollection(x => x, 
                ListContainsSearchKeyword, 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToSourceCommand = ReactiveCommand.Create();
		    GoToSourceCommand.OfType<BranchModel>().Subscribe(x =>
		    {
		        var vm = CreateViewModel<SourceTreeViewModel>();
		        vm.Username = RepositoryOwner;
		        vm.Repository = RepositoryName;
		        vm.Branch = x.Name;
		        vm.TrueBranch = true;
                ShowViewModel(vm);
		    });
            GoToSourceCommand.OfType<TagModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<SourceTreeViewModel>();
                vm.Username = RepositoryOwner;
                vm.Repository = RepositoryName;
                vm.Branch = x.Commit.Sha;
                ShowViewModel(vm);
            });


		    this.WhenAnyValue(x => x.SelectedFilter).Skip(1).Subscribe(_ => LoadCommand.ExecuteIfCan());

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
		    {
                if (SelectedFilter == ShowIndex.Branches)
                {
                    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetBranches();
                    return this.RequestModel(request, t as bool?, response => items.Reset(response.Data));
                }
                else
                {
                    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetTags();
                    return this.RequestModel(request, t as bool?, response => items.Reset(response.Data));
                }
		    });
		}

        private bool ListContainsSearchKeyword(object x)
        {
            var tagModel = x as TagModel;
            if (tagModel != null)
                return tagModel.Name.ContainsKeyword(SearchKeyword);
            var branchModel = x as BranchModel;
            if (branchModel != null)
                return branchModel.Name.ContainsKeyword(SearchKeyword);
            return false;
        }

	    public enum ShowIndex
	    {
	        Branches = 0,
            Tags = 1
	    }
	}
}


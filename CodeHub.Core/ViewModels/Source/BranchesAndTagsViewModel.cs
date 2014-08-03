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

		public ReactiveList<ViewObject> Items { get; private set; }

        public IReactiveCommand<object> GoToSourceCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

		public BranchesAndTagsViewModel(IApplicationService applicationService)
		{
            Items = new ReactiveList<ViewObject>();

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
                    return this.RequestModel(request, t as bool?, response =>
                    {
                        this.CreateMore(response, m => { }, d => Items.AddRange(d.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x })));
                        Items.Reset(response.Data.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x }));
                    });
                }
                else
                {
                    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetTags();
                    return this.RequestModel(request, t as bool?, response =>
                    {
                        this.CreateMore(response, m => { }, d => Items.AddRange(d.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x })));
                        Items.Reset(response.Data.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x }));
                    });
                }

		    });
		}

        public class ViewObject
        {
            public string Name { get; set; }
            public object Object { get; set; }
        }

	    public enum ShowIndex
	    {
	        Branches = 0,
            Tags = 1
	    }
	}
}


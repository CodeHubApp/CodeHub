using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.ViewModels.Source;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.User;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryViewModel : LoadableViewModel
    {
        protected readonly IApplicationService ApplicationService;
        private bool? _starred;
        private bool? _watched;
        private RepositoryModel _repository;
        private ContentModel _readme;
        private List<BranchModel> _branches;
        private string _imageUrl;

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public string ImageUrl
        {
            get { return _imageUrl; }
            set { this.RaiseAndSetIfChanged(ref _imageUrl, value); }
        }

        public bool? IsStarred
        {
            get { return _starred; }
            private set { this.RaiseAndSetIfChanged(ref _starred, value); }
        }

        public bool? IsWatched
        {
            get { return _watched; }
            private set { this.RaiseAndSetIfChanged(ref _watched, value); }
        }

        public RepositoryModel Repository
        {
            get { return _repository; }
            private set { this.RaiseAndSetIfChanged(ref _repository, value); }
        }

        public ContentModel Readme
        {
            get { return _readme; }
            private set { this.RaiseAndSetIfChanged(ref _readme, value); }
        }

        public List<BranchModel> Branches
        {
            get { return _branches; }
            private set { this.RaiseAndSetIfChanged(ref _branches, value); }
        }

        public IReactiveCommand GoToOwnerCommand { get; private set; }

        public IReactiveCommand GoToForkParentCommand { get; private set; }

        public IReactiveCommand GoToStargazersCommand { get; private set; }

        public IReactiveCommand GoToEventsCommand { get; private set; }

        public IReactiveCommand GoToIssuesCommand { get; private set; }

        public IReactiveCommand GoToReadmeCommand { get; private set; }

        public IReactiveCommand GoToCommitsCommand { get; private set; }

        public IReactiveCommand GoToPullRequestsCommand { get; private set; }

        public IReactiveCommand GoToSourceCommand { get; private set; }

        public IReactiveCommand GoToHtmlUrlCommand { get; private set; }

        public bool IsPinned
        {
            get { return ApplicationService.Account.PinnnedRepositories.GetPinnedRepository(RepositoryOwner, RepositoryName) != null; }
        }

        public IReactiveCommand PinCommand { get; private set; }

        public IReactiveCommand ToggleStarCommand { get; private set; }

        public IReactiveCommand ToggleWatchCommand { get; private set; }

        public RepositoryViewModel(IApplicationService applicationService)
        {
            ApplicationService = applicationService;

            ToggleStarCommand = new ReactiveCommand(this.WhenAnyValue(x => x.IsStarred, x => x.HasValue));
            ToggleStarCommand.RegisterAsyncTask(t => ToggleStar());

            ToggleWatchCommand = new ReactiveCommand(this.WhenAnyValue(x => x.IsWatched, x => x.HasValue));
            ToggleWatchCommand.RegisterAsyncTask(t => ToggleWatch());

            GoToOwnerCommand = new ReactiveCommand();
            GoToOwnerCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<ProfileViewModel>();
                vm.Username = RepositoryOwner;
                ShowViewModel(vm);
            });

            PinCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Repository, x => x != null));
            PinCommand.Subscribe(x => PinRepository());

            GoToForkParentCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Repository, x => x != null && x.Fork && x.Parent != null));
            GoToForkParentCommand.Subscribe(x =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = Repository.Parent.Owner.Login;
                vm.RepositoryName = Repository.Parent.Name;
                vm.Repository = Repository.Parent;
                ShowViewModel(vm);
            });

            GoToStargazersCommand = new ReactiveCommand();
            GoToStargazersCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<StargazersViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToEventsCommand = new ReactiveCommand();
            GoToEventsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<RepositoryEventsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToIssuesCommand = new ReactiveCommand();
            GoToIssuesCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<IssuesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToReadmeCommand = new ReactiveCommand();
            GoToReadmeCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<ReadmeViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToCommitsCommand = new ReactiveCommand();
            GoToCommitsCommand.Subscribe(_ =>
            {
                if (Branches != null && Branches.Count == 1)
                {
                    var vm = CreateViewModel<ChangesetsViewModel>();
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    ShowViewModel(vm);
                }
                else
                {
                    var vm = CreateViewModel<ChangesetBranchesViewModel>();
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    ShowViewModel(vm);
                }
            });

            GoToPullRequestsCommand = new ReactiveCommand();
            GoToPullRequestsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<PullRequestsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToSourceCommand = new ReactiveCommand();
            GoToSourceCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<BranchesAndTagsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToHtmlUrlCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Repository, x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)));
            GoToHtmlUrlCommand.Subscribe(_ => GoToUrlCommand.ExecuteIfCan(Repository.HtmlUrl));

            LoadCommand.RegisterAsyncTask(t =>
            {
                var forceCacheInvalidation = t as bool?;

                var t1 = this.RequestModel(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Get(), 
                    forceCacheInvalidation, response => Repository = response.Data);

                this.RequestModel(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetReadme(),
                    forceCacheInvalidation, response => Readme = response.Data).FireAndForget();

                this.RequestModel(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetBranches(),
                    forceCacheInvalidation, response => Branches = response.Data).FireAndForget();

                this.RequestModel(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].IsWatching(),
                    forceCacheInvalidation, response => IsWatched = response.Data).FireAndForget();

                this.RequestModel(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].IsStarred(),
                    forceCacheInvalidation, response => IsStarred = response.Data).FireAndForget();

                return t1;
            });
        }

        private void PinRepository()
        {
            var repoOwner = Repository.Owner.Login;
            var repoName = Repository.Name;

            //Is it pinned already or not?
			var pinnedRepo = ApplicationService.Account.PinnnedRepositories.GetPinnedRepository(repoOwner, repoName);
            if (pinnedRepo == null)
				ApplicationService.Account.PinnnedRepositories.AddPinnedRepository(repoOwner, repoName, repoName, ImageUrl);
            else
				ApplicationService.Account.PinnnedRepositories.RemovePinnedRepository(pinnedRepo.Id);
        }

		private async Task ToggleWatch()
        {
            if (!IsWatched.HasValue)
                return;
	
	        if (IsWatched.Value)
				await ApplicationService.Client.ExecuteAsync(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].StopWatching());
	        else
				await ApplicationService.Client.ExecuteAsync(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Watch());
			IsWatched = !IsWatched.Value;
        }

		private async Task ToggleStar()
		{
		    if (!IsStarred.HasValue)
		        return;

            if (IsStarred.Value)
                await ApplicationService.Client.ExecuteAsync(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Unstar());
	        else
                await ApplicationService.Client.ExecuteAsync(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Star());
	        IsStarred = !IsStarred.Value;
        }
    }
}


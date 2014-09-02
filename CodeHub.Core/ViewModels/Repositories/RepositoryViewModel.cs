using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.ViewModels.Source;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Users;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using System.Reactive.Linq;
using CodeHub.Core.Data;
using CodeHub.Core.ViewModels.Organizations;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryViewModel : BaseViewModel, ILoadableViewModel
    {
        protected readonly IApplicationService ApplicationService;
        private readonly IAccountsService _accountsService;
        private bool? _starred;
        private bool? _watched;
        private RepositoryModel _repository;
        private ContentModel _readme;
        private List<BranchModel> _branches;
        private int? _collaborators;
        private string _repositorySize;

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

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

        public int? Collaborators
        {
            get { return _collaborators; }
            private set { this.RaiseAndSetIfChanged(ref _collaborators, value); }
        }

        public string RepositorySize
        {
            get { return _repositorySize; }
            private set { this.RaiseAndSetIfChanged(ref _repositorySize, value); }
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

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToOwnerCommand { get; private set; }

        public IReactiveCommand<object> GoToForkParentCommand { get; private set; }

        public IReactiveCommand<object> GoToStargazersCommand { get; private set; }

        public IReactiveCommand<object> GoToWatchersCommand { get; private set; }

        public IReactiveCommand<object> GoToCollaboratorsCommand { get; private set; }

        public IReactiveCommand<object> GoToEventsCommand { get; private set; }

        public IReactiveCommand<object> GoToIssuesCommand { get; private set; }

        public IReactiveCommand<object> GoToReadmeCommand { get; private set; }

        public IReactiveCommand<object> GoToCommitsCommand { get; private set; }

        public IReactiveCommand<object> GoToPullRequestsCommand { get; private set; }

        public IReactiveCommand<object> GoToSourceCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public IReactiveCommand GoToForksCommand { get; private set; }

        public bool IsPinned
        {
            get
            {
                return ApplicationService.Account.PinnnedRepositories.Any(x => x.Owner.Equals(RepositoryOwner) && x.Name.Equals(RepositoryName));
            }
        }

        public IReactiveCommand<object> PinCommand { get; private set; }

        public IReactiveCommand ToggleStarCommand { get; private set; }

        public IReactiveCommand ToggleWatchCommand { get; private set; }

        public RepositoryViewModel(IApplicationService applicationService, IAccountsService accountsService)
        {
            ApplicationService = applicationService;
            _accountsService = accountsService;

            ToggleStarCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsStarred).Select(x => x.HasValue), t => ToggleStar());

            ToggleWatchCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsWatched, x => x.HasValue), t => ToggleWatch());

            GoToOwnerCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository).Select(x => x != null));
            GoToOwnerCommand.Select(_ => Repository.Owner).Subscribe(x =>
            {
                if (string.Equals(x.Type, "organization", StringComparison.OrdinalIgnoreCase))
                {
                    var vm = CreateViewModel<OrganizationViewModel>();
                    vm.Username = RepositoryOwner;
                    ShowViewModel(vm);
                }
                else
                {
                    var vm = CreateViewModel<UserViewModel>();
                    vm.Username = RepositoryOwner;
                    ShowViewModel(vm);
                }
            });

            PinCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository).Select(x => x != null));
            PinCommand.Subscribe(x => PinRepository());

            GoToForkParentCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository, x => x != null && x.Fork && x.Parent != null));
            GoToForkParentCommand.Subscribe(x =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = Repository.Parent.Owner.Login;
                vm.RepositoryName = Repository.Parent.Name;
                vm.Repository = Repository.Parent;
                ShowViewModel(vm);
            });

            GoToStargazersCommand = ReactiveCommand.Create();
            GoToStargazersCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<RepositoryStargazersViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToWatchersCommand = ReactiveCommand.Create();
            GoToWatchersCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<RepositoryWatchersViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToEventsCommand = ReactiveCommand.Create();
            GoToEventsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<RepositoryEventsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToIssuesCommand = ReactiveCommand.Create();
            GoToIssuesCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<IssuesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToReadmeCommand = ReactiveCommand.Create();
            GoToReadmeCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<ReadmeViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToCommitsCommand = ReactiveCommand.Create();
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

            GoToPullRequestsCommand = ReactiveCommand.Create();
            GoToPullRequestsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<PullRequestsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToSourceCommand = ReactiveCommand.Create();
            GoToSourceCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<BranchesAndTagsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToCollaboratorsCommand = ReactiveCommand.Create();
            GoToCollaboratorsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<RepositoryCollaboratorsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToForksCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<RepositoryForksViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            this.WhenAnyValue(x => x.Repository).Subscribe(x =>
            {
                if (x == null)
                    RepositorySize = null;
                else
                {
                    if (x.Size / 1024f < 1)
                        RepositorySize = string.Format("{0:0.##}KB", x.Size);
                    else if ((x.Size / 1024f / 1024f) < 1)
                        RepositorySize = string.Format("{0:0.##}MB", x.Size / 1024f);
                    else
                        RepositorySize = string.Format("{0:0.##}GB", x.Size / 1024f / 1024f);
                }
            });

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository, x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)));
            GoToHtmlUrlCommand.Subscribe(_ => GoToUrlCommand.ExecuteIfCan(Repository.HtmlUrl));

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
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

                this.RequestModel(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetCollaborators(),
                    forceCacheInvalidation, response => Collaborators = response.Data.Count).FireAndForget();

                return t1;
            });
        }

        private void PinRepository()
        {
            var repoOwner = Repository.Owner.Login;
            var repoName = Repository.Name;

            //Is it pinned already or not?
			var pinnedRepo = ApplicationService.Account.PinnnedRepositories.FirstOrDefault(x => x.Owner.Equals(repoOwner) && x.Name.Equals(repoName));
            if (pinnedRepo == null)
            {
                ApplicationService.Account.PinnnedRepositories.Add(new PinnedRepository
                {
                    Owner = repoOwner,
                    Name = repoName,
                    ImageUri = Repository.Owner.AvatarUrl,
                    Slug = Repository.FullName
                });
                _accountsService.Update(ApplicationService.Account);
            }
            else
                ApplicationService.Account.PinnnedRepositories.Remove(pinnedRepo);
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


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
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Data;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.Core.ViewModels.Releases;
using System.Reactive;
using CodeHub.Core.ViewModels.Contents;
using CodeHub.Core.Factories;
using CodeHub.Core.ViewModels.Activity;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryViewModel : BaseViewModel, ILoadableViewModel
    {
        protected readonly ISessionService ApplicationService;
        private readonly IAccountsRepository _accountsService;
        private bool? _starred;
        private bool? _watched;
        private RepositoryModel _repository;
        private ContentModel _readme;
        private List<BranchModel> _branches;
        private int? _contributors;

        public string RepositoryOwner { get; set; }

        private string _repositoryName;
        public string RepositoryName
        {
            get { return _repositoryName; }
            set { this.RaiseAndSetIfChanged(ref _repositoryName, value); }
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

        public int? Contributors
        {
            get { return _contributors; }
            private set { this.RaiseAndSetIfChanged(ref _contributors, value); }
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

        private int? _languages;
        public int? Languages
        {
            get { return _languages; }
            private set { this.RaiseAndSetIfChanged(ref _languages, value); }
        }

        private int? _releases;
        public int? Releases
        {
            get { return _releases; }
            private set { this.RaiseAndSetIfChanged(ref _releases, value); }
        }

        private int? _stargazers;
        public int? Stargazers
        {
            get { return _stargazers; }
            private set { this.RaiseAndSetIfChanged(ref _stargazers, value); }
        }

        private int? _watchers;
        public int? Watchers
        {
            get { return _watchers; }
            private set { this.RaiseAndSetIfChanged(ref _watchers, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToOwnerCommand { get; private set; }

        public IReactiveCommand<object> GoToForkParentCommand { get; private set; }

        public IReactiveCommand<object> GoToStargazersCommand { get; private set; }

        public IReactiveCommand<object> GoToWatchersCommand { get; private set; }

        public IReactiveCommand<object> GoToContributors { get; private set; }

        public IReactiveCommand<object> GoToEventsCommand { get; private set; }

        public IReactiveCommand<object> GoToIssuesCommand { get; private set; }

        public IReactiveCommand<object> GoToReadmeCommand { get; private set; }

        public IReactiveCommand<object> GoToCommitsCommand { get; private set; }

        public IReactiveCommand<object> GoToBranchesCommand { get; private set; }

        public IReactiveCommand<object> GoToPullRequestsCommand { get; private set; }

        public IReactiveCommand<object> GoToSourceCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public IReactiveCommand GoToReleasesCommand { get; private set; }

        public IReactiveCommand GoToForksCommand { get; private set; }

        public IReactiveCommand<object> GoToHomepageCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

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

        public RepositoryViewModel(ISessionService applicationService, 
            IAccountsRepository accountsService, IActionMenuFactory actionMenuService)
        {
            ApplicationService = applicationService;
            _accountsService = accountsService;

            this.WhenAnyValue(x => x.RepositoryName).Subscribe(x => Title = x);

            this.WhenAnyValue(x => x.Repository).Subscribe(x => 
            {
                Stargazers = x != null ? (int?)x.StargazersCount : null;
                Watchers = x != null ? (int?)x.SubscribersCount : null;
            });

            ToggleStarCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsStarred).Select(x => x.HasValue), t => ToggleStar());

            ToggleWatchCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsWatched, x => x.HasValue), t => ToggleWatch());

            GoToOwnerCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository).Select(x => x != null));
            GoToOwnerCommand.Select(_ => Repository.Owner).Subscribe(x => {
                if (string.Equals(x.Type, "organization", StringComparison.OrdinalIgnoreCase))
                {
                    var vm = this.CreateViewModel<OrganizationViewModel>();
                    vm.Init(RepositoryOwner);
                    NavigateTo(vm);
                }
                else
                {
                    var vm = this.CreateViewModel<UserViewModel>();
                    vm.Init(RepositoryOwner);
                    NavigateTo(vm);
                }
            });

            PinCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository).Select(x => x != null));
            PinCommand.Subscribe(x => PinRepository());

            GoToForkParentCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository, x => x != null && x.Fork && x.Parent != null));
            GoToForkParentCommand.Subscribe(x =>
            {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = Repository.Parent.Owner.Login;
                vm.RepositoryName = Repository.Parent.Name;
                vm.Repository = Repository.Parent;
                NavigateTo(vm);
            });

            GoToStargazersCommand = ReactiveCommand.Create();
            GoToStargazersCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<RepositoryStargazersViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            GoToWatchersCommand = ReactiveCommand.Create();
            GoToWatchersCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<RepositoryWatchersViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            GoToEventsCommand = ReactiveCommand.Create();
            GoToEventsCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<RepositoryEventsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            GoToIssuesCommand = ReactiveCommand.Create();
            GoToIssuesCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<IssuesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            GoToReadmeCommand = ReactiveCommand.Create();
            GoToReadmeCommand
                .Select(_ => this.CreateViewModel<ReadmeViewModel>())
                .Select(x => x.Init(RepositoryOwner, RepositoryName))
                .Subscribe(NavigateTo);

            GoToBranchesCommand = ReactiveCommand.Create();
            GoToBranchesCommand
                .Select(_ => this.CreateViewModel<CommitBranchesViewModel>())
                .Select(x => x.Init(RepositoryOwner, RepositoryName))
                .Subscribe(NavigateTo);

            GoToCommitsCommand = ReactiveCommand.Create();
            GoToCommitsCommand.Subscribe(_ =>
            {
                if (Branches != null && Branches.Count == 1)
                {
                    var vm = this.CreateViewModel<CommitsViewModel>();
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    vm.Branch = Repository == null ? null : Repository.DefaultBranch;
                    NavigateTo(vm);
                }
                else
                {
                    GoToBranchesCommand.ExecuteIfCan();
                }
            });

            GoToPullRequestsCommand = ReactiveCommand.Create();
            GoToPullRequestsCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<PullRequestsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            GoToSourceCommand = ReactiveCommand.Create();
            GoToSourceCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<BranchesAndTagsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            GoToContributors = ReactiveCommand.Create();
            GoToContributors.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<RepositoryContributorsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            GoToForksCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<RepositoryForksViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            GoToReleasesCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<ReleasesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            var canShowMenu = this.WhenAnyValue(x => x.Repository, x => x.IsStarred, x => x.IsWatched)
                .Select(x => x.Item1 != null && x.Item2 != null && x.Item3 != null);

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(canShowMenu, sender => {
                var menu = actionMenuService.Create(Title);
                menu.AddButton(IsPinned ? "Unpin from Slideout Menu" : "Pin to Slideout Menu", PinCommand);
                menu.AddButton(IsStarred.Value ? "Unstar This Repo" : "Star This Repo", ToggleStarCommand);
                menu.AddButton(IsWatched.Value ? "Unwatch This Repo" : "Watch This Repo", ToggleWatchCommand);
                menu.AddButton("Show in GitHub", GoToHtmlUrlCommand);
                return menu.Show(sender);
            });

            var gotoWebUrl = new Action<string>(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;
                NavigateTo(vm);
            });

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository, x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)));
            GoToHtmlUrlCommand.Select(_ => Repository.HtmlUrl).Subscribe(gotoWebUrl);

            GoToHomepageCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository, x => x != null && !string.IsNullOrEmpty(x.Homepage)));
            GoToHomepageCommand.Select(_ => Repository.Homepage).Subscribe(gotoWebUrl);

            LoadCommand = ReactiveCommand.CreateAsyncTask(_ => {

                var t1 = this.RequestModel(ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Get(), 
                    true, response => Repository = response.Data);

                applicationService.Client.ExecuteAsync(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetReadme())
                    .ToBackground(x => Readme = x.Data);

                applicationService.Client.ExecuteAsync(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetBranches())
                    .ToBackground(x => Branches = x.Data);

                applicationService.GitHubClient.Activity.Watching.CheckWatched(RepositoryOwner, RepositoryName)
                    .ToBackground(x => IsWatched = x);

                applicationService.GitHubClient.Activity.Starring.CheckStarred(RepositoryOwner, RepositoryName)
                    .ToBackground(x => IsStarred = x);

                applicationService.Client.ExecuteAsync(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetContributors())
                    .ToBackground(x => Contributors = x.Data.Count);

                applicationService.Client.ExecuteAsync(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetLanguages())
                    .ToBackground(x => Languages = x.Data.Count);

                applicationService.Client.ExecuteAsync(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetReleases())
                    .ToBackground(x => Releases = x.Data.Count);

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

            if (Watchers.HasValue)
                Watchers += (IsWatched.Value ? -1 : 1);
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

            if (Stargazers.HasValue)
                Stargazers += (IsStarred.Value ? -1 : 1);
            IsStarred = !IsStarred.Value;
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;
        
        public string Username { get; private set; }

        public string RepositoryName { get; private set; }

        public string ImageUrl { get; set; }

        private bool? _starred;
        public bool? IsStarred
        {
            get { return _starred; }
            private set { this.RaiseAndSetIfChanged(ref _starred, value); }
        }

        private bool? _watched;
        public bool? IsWatched
        {
            get { return _watched; }
            private set { this.RaiseAndSetIfChanged(ref _watched, value); }
        }

        private Octokit.Repository _repository;
        public Octokit.Repository Repository
        {
            get { return _repository; }
            set { this.RaiseAndSetIfChanged(ref _repository, value); }
        }

        private Octokit.Readme _readme;
        public Octokit.Readme Readme
        {
            get { return _readme; }
            private set { this.RaiseAndSetIfChanged(ref _readme, value); }
        }

        private IReadOnlyList<Octokit.Branch> _branches;
        public IReadOnlyList<Octokit.Branch> Branches
        {
            get { return _branches; }
            private set { this.RaiseAndSetIfChanged(ref _branches, value); }
        }
 
        public ReactiveCommand<Unit, Unit> ToggleWatchCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleStarCommand { get; }

        public ReactiveCommand<Unit, Unit> PinCommand { get; }

        public RepositoryViewModel(
            string username,
            string repository,
            IApplicationService applicationService = null)
        {
            Username = username;
            RepositoryName = repository;
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            ToggleWatchCommand = ReactiveCommand.CreateFromTask(
                ToggleWatch,
                this.WhenAnyValue(x => x.IsWatched).Select(x => x != null));

            ToggleStarCommand = ReactiveCommand.CreateFromTask(
                ToggleStar,
                this.WhenAnyValue(x => x.IsStarred).Select(x => x != null));

            ToggleStarCommand
                .ThrownExceptions
                .Select(err => new UserError("Unable to " + (IsStarred.GetValueOrDefault() ? "unstar" : "star") + " this repository! Please try again.", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            ToggleWatchCommand
                .ThrownExceptions
                .Select(err => new UserError("Unable to toggle repository as " + (IsWatched.GetValueOrDefault() ? "unwatched" : "watched") + "! Please try again.", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            PinCommand = ReactiveCommand.CreateFromTask(
                PinRepository,
                this.WhenAnyValue(x => x.Repository).Select(x => x != null));

            PinCommand
                .ThrownExceptions
                .Select(err => new UserError("Failed to pin repository!", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();
        }


        private async Task PinRepository()
        {
            var repoOwner = Repository.Owner.Login;
            var repoName = Repository.Name;
            var account = this.GetApplication().Account;
            var pinnedRepository = 
                account.PinnedRepositories
                    .FirstOrDefault(x => string.Equals(repoName, x.Name, StringComparison.OrdinalIgnoreCase) &&
                                         string.Equals(repoOwner, x.Owner, StringComparison.OrdinalIgnoreCase));

            //Is it pinned already or not?
            if (pinnedRepository == null)
            {
                account.PinnedRepositories.Add(new Data.PinnedRepository
                {
                    Name = repoName,
                    Owner = repoOwner,
                    ImageUri = ImageUrl,
                    Slug = repoName
                });

                await _applicationService.UpdateActiveAccount();
            }
            else
            {
                account.PinnedRepositories.Remove(pinnedRepository);
                await _applicationService.UpdateActiveAccount();
            }
        }


        protected override async Task Load()
        {
            _applicationService.GitHubClient.Repository.Content
                 .GetReadme(Username, RepositoryName).ToBackground(x => Readme = x);

            _applicationService.GitHubClient.Repository.Branch
                 .GetAll(Username, RepositoryName).ToBackground(x => Branches = x);

            _applicationService.GitHubClient.Activity.Starring
                .CheckStarred(Username, RepositoryName).ToBackground(x => IsStarred = x);

            _applicationService.GitHubClient.Activity.Watching
                .CheckWatched(Username, RepositoryName).ToBackground(x => IsWatched = x);

            var retrieveRepository = _applicationService.GitHubClient.Repository.Get(Username, RepositoryName);

            if (Repository == null)
                Repository = await retrieveRepository;
            else
                retrieveRepository.ToBackground(repo => Repository = repo);
        }

        private async Task ToggleWatch()
        {
            if (IsWatched == null)
                return;

            if (IsWatched.Value)
                await this.GetApplication().GitHubClient.Activity.Watching.UnwatchRepo(Username, RepositoryName);
            else
            {
                var subscription = new Octokit.NewSubscription()
                {
                    Subscribed = true
                };

                await this.GetApplication().GitHubClient.Activity.Watching.WatchRepo(Username, RepositoryName, subscription);
            }
            IsWatched = !IsWatched;
        }

        public bool IsPinned
        {
            get 
            {
                var repos = this.GetApplication().Account.PinnedRepositories;
                return repos.Any(x => string.Equals(x.Owner, Username, StringComparison.OrdinalIgnoreCase) &&
                                 string.Equals(x.Slug, RepositoryName, StringComparison.OrdinalIgnoreCase));
            }
        }

        private async Task ToggleStar()
        {
            if (IsStarred == null)
                return;

            if (IsStarred.Value)
                await this.GetApplication().GitHubClient.Activity.Starring.RemoveStarFromRepo(Username, RepositoryName);
            else
                await this.GetApplication().GitHubClient.Activity.Starring.StarRepo(Username, RepositoryName);
            IsStarred = !IsStarred;
        }
    }
}


using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.User;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Changesets;
using System.Linq;
using System;
using CodeHub.Core.Services;
using Splat;
using CodeHub.Core.ViewModels.Source;

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

        public RepositoryViewModel(IApplicationService applicationService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Title = RepositoryName = navObject.Repository;
        }

        public ICommand GoToEventsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryEventsViewModel>(new RepositoryEventsViewModel.NavObject { Username = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToIssuesCommand
        {
            get { return new MvxCommand(() => ShowViewModel<Issues.IssuesViewModel>(new Issues.IssuesViewModel.NavObject { Username = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToPullRequestsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<PullRequests.PullRequestsViewModel>(new PullRequests.PullRequestsViewModel.NavObject { Username = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToHtmlUrlCommand
        {
            get { return new MvxCommand(() => ShowViewModel<WebBrowserViewModel>(new WebBrowserViewModel.NavObject { Url = Repository.HtmlUrl }), () => Repository != null); }
        }

        public ICommand PinCommand
        {
            get { return new MvxCommand(PinRepository, () => Repository != null); }
        }

        private void PinRepository()
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

                _applicationService.UpdateActiveAccount().ToBackground();
                
            }
            else
            {
                account.PinnedRepositories.Remove(pinnedRepository);
                _applicationService.UpdateActiveAccount().ToBackground();
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

        public ICommand ToggleWatchCommand
        {
            get { return new MvxCommand(() => ToggleWatch(), () => IsWatched != null); }
        }

        private async Task ToggleWatch()
        {
            if (IsWatched == null)
                return;

            try
            {
                if (IsWatched.Value)
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].StopWatching());
                else
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Watch());
                IsWatched = !IsWatched;
            }
            catch
            {
                DisplayAlert("Unable to toggle repository as " + (IsWatched.Value ? "unwatched" : "watched") + "! Please try again.");
            }
        }

        public ICommand ToggleStarCommand
        {
            get { return new MvxCommand(() => ToggleStar(), () => IsStarred != null); }
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

            try
            {
                if (IsStarred.Value)
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Unstar());
                else
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Star());
                IsStarred = !IsStarred;
            }
            catch
            {
                DisplayAlert("Unable to " + (IsStarred.Value ? "unstar" : "star") + " this repository! Please try again.");
            }
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}


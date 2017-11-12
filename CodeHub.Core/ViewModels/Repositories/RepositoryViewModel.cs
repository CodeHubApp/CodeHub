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

        private RepositoryModel _repository;
        public RepositoryModel Repository
        {
            get { return _repository; }
            private set { this.RaiseAndSetIfChanged(ref _repository, value); }
        }

        private ContentModel _readme;
        public ContentModel Readme
        {
            get { return _readme; }
            private set { this.RaiseAndSetIfChanged(ref _readme, value); }
        }

        private List<BranchModel> _branches;
        public List<BranchModel> Branches
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

        public ICommand GoToOwnerCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserViewModel>(new UserViewModel.NavObject { Username = Username })); }
        }

        public ICommand GoToForkParentCommand
        {
            get { return new MvxCommand<RepositoryModel>(x => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner.Login, Repository = x.Name })); }
        }

        public ICommand GoToEventsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryEventsViewModel>(new RepositoryEventsViewModel.NavObject { Username = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToIssuesCommand
        {
            get { return new MvxCommand(() => ShowViewModel<Issues.IssuesViewModel>(new Issues.IssuesViewModel.NavObject { Username = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToReadmeCommand
        {
            get { return new MvxCommand(() => ShowViewModel<ReadmeViewModel>(new ReadmeViewModel.NavObject { Username = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToCommitsCommand
        {
            get { return new MvxCommand(ShowCommits);}
        }

        public ICommand GoToPullRequestsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<PullRequests.PullRequestsViewModel>(new PullRequests.PullRequestsViewModel.NavObject { Username = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToHtmlUrlCommand
        {
            get { return new MvxCommand(() => ShowViewModel<WebBrowserViewModel>(new WebBrowserViewModel.NavObject { Url = Repository.HtmlUrl }), () => Repository != null); }
        }

        private void ShowCommits()
        {
            if (Branches?.Count == 1)
                ShowViewModel<ChangesetsViewModel>(new ChangesetsViewModel.NavObject {Username = Username, Repository = RepositoryName});
            else
                ShowViewModel<ChangesetBranchesViewModel>(new ChangesetBranchesViewModel.NavObject {Username = Username, Repository = RepositoryName});
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


        protected override Task Load()
        {
            var t1 = this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Get(), response => Repository = response.Data);

            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].GetReadme(), 
                response => Readme = response.Data).ToBackground();

            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].GetBranches(), 
                response => Branches = response.Data).ToBackground();

            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].IsWatching(), 
                response => IsWatched = response.Data).ToBackground();
         
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].IsStarred(), 
                response => IsStarred = response.Data).ToBackground();

            return t1;
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


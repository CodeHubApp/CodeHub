using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.User;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryViewModel : LoadableViewModel
    {
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

        public ICommand GoToStargazersCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryStargazersViewModel>(new RepositoryStargazersViewModel.NavObject { User = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToWatchersCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryWatchersViewModel>(new RepositoryWatchersViewModel.NavObject { User = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToForkedCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoriesForkedViewModel>(new RepositoriesForkedViewModel.NavObject { User = Username, Repository = RepositoryName })); }
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

        public ICommand GoToSourceCommand
        {
            get { return new MvxCommand(() => ShowViewModel<Source.BranchesAndTagsViewModel>(new Source.BranchesAndTagsViewModel.NavObject { Username = Username, Repository = RepositoryName })); }
        }

        public ICommand GoToHtmlUrlCommand
        {
            get { return new MvxCommand(() => ShowViewModel<WebBrowserViewModel>(new WebBrowserViewModel.NavObject { Url = Repository.HtmlUrl }), () => Repository != null); }
        }

        private void ShowCommits()
        {
            if (Branches != null && Branches.Count == 1)
                ShowViewModel<ChangesetsViewModel>(new ChangesetsViewModel.NavObject {Username = Username, Repository = RepositoryName});
            else
                ShowViewModel<Source.ChangesetBranchesViewModel>(new Source.ChangesetBranchesViewModel.NavObject {Username = Username, Repository = RepositoryName});
        }

        public ICommand PinCommand
        {
            get { return new MvxCommand(PinRepository, () => Repository != null); }
        }

        private void PinRepository()
        {
            var repoOwner = Repository.Owner.Login;
            var repoName = Repository.Name;

            //Is it pinned already or not?
            var pinnedRepo = this.GetApplication().Account.PinnnedRepositories.GetPinnedRepository(repoOwner, repoName);
            if (pinnedRepo == null)
                this.GetApplication().Account.PinnnedRepositories.AddPinnedRepository(repoOwner, repoName, repoName, ImageUrl);
            else
                this.GetApplication().Account.PinnnedRepositories.RemovePinnedRepository(pinnedRepo.Id);
        }


        protected override Task Load()
        {
            var t1 = this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Get(), response => Repository = response.Data);

            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].GetReadme(), 
                response => Readme = response.Data).FireAndForget();

            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].GetBranches(), 
                response => Branches = response.Data).FireAndForget();

            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].IsWatching(), 
                response => IsWatched = response.Data).FireAndForget();
         
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].IsStarred(), 
                response => IsStarred = response.Data).FireAndForget();

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
            get { return this.GetApplication().Account.PinnnedRepositories.GetPinnedRepository(Username, RepositoryName) != null; }
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


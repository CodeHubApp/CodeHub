using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.User;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryViewModel : LoadableViewModel
    {
        private bool? _starred;
        private bool? _watched;
        private RepositoryModel _repository;
        private ContentModel _readme;
        private List<BranchModel> _branches;

        public string Username 
        { 
            get; 
            private set; 
        }

        public string RepositoryName 
        { 
            get; 
            private set; 
        }

		public string ImageUrl
		{
			get;
			set;
		}

        public bool? IsStarred
        {
            get { return _starred; }
            private set
            {
                _starred = value;
                RaisePropertyChanged(() => IsStarred);
            }
        }

        public bool? IsWatched
        {
            get { return _watched; }
            private set
            {
                _watched = value;
                RaisePropertyChanged(() => IsWatched);
            }
        }

        public RepositoryModel Repository
        {
            get { return _repository; }
            private set
            {
                _repository = value;
                RaisePropertyChanged(() => Repository);
            }
        }

        public ContentModel Readme
        {
            get { return _readme; }
            private set
            {
                _readme = value;
                RaisePropertyChanged(() => Readme);
            }
        }

        public List<BranchModel> Branches
        {
            get { return _branches; }
            private set
            {
                _branches = value;
                RaisePropertyChanged(() => Branches);
            }
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            RepositoryName = navObject.Repository;
        }

		public ICommand GoToOwnerCommand
		{
			get { return new MvxCommand(() => ShowViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = Username })); }
		}

		public ICommand GoToForkParentCommand
		{
			get { return new MvxCommand<RepositoryModel>(x => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner.Login, Repository = x.Name })); }
		}

		public ICommand GoToStargazersCommand
		{
			get { return new MvxCommand(() => ShowViewModel<StargazersViewModel>(new StargazersViewModel.NavObject { User = Username, Repository = RepositoryName })); }
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


        protected override Task Load(bool forceCacheInvalidation)
        {
			var t1 = this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Get(), forceCacheInvalidation, response => Repository = response.Data);

			this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].GetReadme(), 
				forceCacheInvalidation, response => Readme = response.Data).FireAndForget();

			this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].GetBranches(), 
				forceCacheInvalidation, response => Branches = response.Data).FireAndForget();

			this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].IsWatching(), 
				forceCacheInvalidation, response => IsWatched = response.Data).FireAndForget();
         
			this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].IsStarred(), 
				forceCacheInvalidation, response => IsStarred = response.Data).FireAndForget();

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
			catch (Exception e)
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
			catch (Exception e)
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


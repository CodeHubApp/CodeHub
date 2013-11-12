using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;

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

        public ICommand ShowCommitsCommand
        {
            get { return new MvxCommand(ShowCommits);}
        }

        private void ShowCommits()
        {
            if (Branches != null && Branches.Count == 1)
                ShowViewModel<ChangesetsViewModel>(new ChangesetsViewModel.NavObject {Username = Username, Repository = RepositoryName});
            else
            {
#warning Need to fix this
                throw new Exception("Need to fix");
                ShowViewModel<ChangesetsViewModel>(new ChangesetsViewModel.NavObject {Username = Username, Repository = RepositoryName});
            }
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
            var pinnedRepo = Application.Account.PinnnedRepositories.GetPinnedRepository(repoOwner, repoName);
            if (pinnedRepo == null)
            {
                //var imageUrl = Repository.Fork ? Images.GitHubRepoForkUrl : CodeHub.Images.GitHubRepoUrl;
                //Application.Account.PinnnedRepositories.AddPinnedRepository(repoOwner, repoName, repoName, imageUrl.AbsolutePath);
            }
            else
                Application.Account.PinnnedRepositories.RemovePinnedRepository(pinnedRepo.Id);
        }


        protected override Task Load(bool forceDataRefresh)
        {
            var t1 = Task.Run(() => this.RequestModel(Application.Client.Users[Username].Repositories[RepositoryName].Get(), forceDataRefresh, response => Repository = response.Data));

            FireAndForgetTask.Start(() => this.RequestModel(Application.Client.Users[Username].Repositories[RepositoryName].GetReadme(), 
                    forceDataRefresh, response => Readme = response.Data));

            FireAndForgetTask.Start(() => this.RequestModel(Application.Client.Users[Username].Repositories[RepositoryName].GetBranches(), 
                    forceDataRefresh, response => Branches = response.Data));

            FireAndForgetTask.Start(() => this.RequestModel(Application.Client.Users[Username].Repositories[RepositoryName].IsWatching(), 
                    forceDataRefresh, response => IsWatched = response.Data));
         
            FireAndForgetTask.Start(() => this.RequestModel(Application.Client.Users[Username].Repositories[RepositoryName].IsStarred(), 
                    forceDataRefresh, response => IsStarred = response.Data));

            return t1;
        }

        public ICommand ToggleWatchCommand
        {
            get { return new MvxCommand(ToggleWatch, () => IsWatched != null); }
        }

        private async void ToggleWatch()
        {
            if (IsWatched == null)
                return;

            if (IsWatched.Value)
                await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[RepositoryName].StopWatching());
            else
                await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[RepositoryName].Watch());

            IsWatched = !IsWatched;
        }

        public ICommand ToggleStarCommand
        {
            get { return new MvxCommand(ToggleStar, () => IsStarred != null); }
        }

        public bool IsPinned
        {
            get { return Application.Account.PinnnedRepositories.GetPinnedRepository(Username, RepositoryName) != null; }
        }

        private async void ToggleStar()
        {
            if (IsStarred == null)
                return;

            if (IsStarred.Value)
                await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[RepositoryName].Unstar());
            else
                await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[RepositoryName].Star());
            IsStarred = !IsStarred;
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}


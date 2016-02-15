using System;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using System.Windows.Input;
using System.Threading.Tasks;
using GitHubSharp;
using System.Collections.Generic;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Changesets
{
	public abstract class CommitsViewModel : LoadableViewModel
	{
		private readonly CollectionViewModel<CommitModel> _commits = new CollectionViewModel<CommitModel>();
        private readonly IFeaturesService _featuresService;

		public string Username
		{
			get;
			private set;
		}

		public string Repository
		{
			get;
			private set;
		}

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

		public ICommand GoToChangesetCommand
		{
			get { return new MvxCommand<CommitModel>(x => ShowViewModel<ChangesetViewModel>(new ChangesetViewModel.NavObject { Username = Username, Repository = Repository, Node = x.Sha })); }
		}

		public CollectionViewModel<CommitModel> Commits
		{
			get { return _commits; }
		}

        protected CommitsViewModel(IFeaturesService featuresService)
        {
            _featuresService = featuresService;
        }

		public void Init(NavObject navObject)
		{
			Username = navObject.Username;
			Repository = navObject.Repository;
		}

		protected override Task Load(bool forceCacheInvalidation)
		{
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
                this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Get(), false, x => ShouldShowPro = x.Data.Private && !_featuresService.IsProEnabled);
            
			return Commits.SimpleCollectionLoad(GetRequest(), forceCacheInvalidation);
		}

		protected abstract GitHubRequest<List<CommitModel>> GetRequest();

		public class NavObject
		{
			public string Username { get; set; }
			public string Repository { get; set; }
		}
	}
}


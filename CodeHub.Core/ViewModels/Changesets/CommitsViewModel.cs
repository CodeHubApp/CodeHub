using System;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Windows.Input;
using System.Threading.Tasks;
using GitHubSharp;
using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;

namespace CodeHub.Core.ViewModels.Changesets
{
	public abstract class CommitsViewModel : LoadableViewModel
	{
		private readonly CollectionViewModel<CommitModel> _commits = new CollectionViewModel<CommitModel>();

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

		public ICommand GoToChangesetCommand
		{
			get { return new MvxCommand<CommitModel>(x => ShowViewModel<ChangesetViewModel>(new ChangesetViewModel.NavObject { Username = Username, Repository = Repository, Node = x.Sha })); }
		}

		public CollectionViewModel<CommitModel> Commits
		{
			get { return _commits; }
		}

		public void Init(NavObject navObject)
		{
			Username = navObject.Username;
			Repository = navObject.Repository;
		}

		protected override Task Load(bool forceCacheInvalidation)
		{
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


using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Changesets
{
	public abstract class CommitsViewModel : LoadableViewModel
	{
		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

		public IReactiveCommand GoToChangesetCommand { get; private set; }

		public ReactiveCollection<CommitModel> Commits { get; private set; }

	    protected CommitsViewModel()
	    {
	        Commits = new ReactiveCollection<CommitModel>();

            GoToChangesetCommand = new ReactiveCommand();
	        GoToChangesetCommand.OfType<CommitModel>().Subscribe(x =>
	        {
	            var vm = CreateViewModel<ChangesetViewModel>();
	            vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
	            vm.Node = x.Sha;
                ShowViewModel(vm);
	        });

	        LoadCommand.RegisterAsyncTask(x => Commits.SimpleCollectionLoad(GetRequest(), x as bool?));
	    }

		protected abstract GitHubRequest<List<CommitModel>> GetRequest();
	}
}


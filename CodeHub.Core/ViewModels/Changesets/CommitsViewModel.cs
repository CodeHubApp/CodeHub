using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Changesets
{
    public abstract class CommitsViewModel : BaseViewModel, ILoadableViewModel
	{
		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public IReactiveCommand<object> GoToChangesetCommand { get; private set; }

		public ReactiveList<CommitModel> Commits { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

	    protected CommitsViewModel()
	    {
	        Commits = new ReactiveList<CommitModel>();

            GoToChangesetCommand = ReactiveCommand.Create();
	        GoToChangesetCommand.OfType<CommitModel>().Subscribe(x =>
	        {
	            var vm = CreateViewModel<ChangesetViewModel>();
	            vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
	            vm.Node = x.Sha;
                ShowViewModel(vm);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => Commits.SimpleCollectionLoad(GetRequest(), x as bool?));
	    }

		protected abstract GitHubRequest<List<CommitModel>> GetRequest();
	}
}


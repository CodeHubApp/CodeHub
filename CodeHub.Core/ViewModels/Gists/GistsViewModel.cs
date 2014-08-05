using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using GitHubSharp;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class GistsViewModel : BaseViewModel, ILoadableViewModel
    {
        protected readonly ReactiveList<GistModel> GistsCollection = new ReactiveList<GistModel>();

        public IReadOnlyReactiveList<GistModel> Gists { get; private set; }

        public IReactiveCommand<object> GoToGistCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        protected GistsViewModel()
        {
            Gists = GistsCollection.CreateDerivedCollection(
                x => x, x => x.Description.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToGistCommand = ReactiveCommand.Create();
            GoToGistCommand.OfType<GistModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<GistViewModel>();
                vm.Id = x.Id;
                vm.Gist = x;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t => GistsCollection.SimpleCollectionLoad(CreateRequest(), t as bool?));
        }

        protected abstract GitHubRequest<List<GistModel>> CreateRequest();
    }
}


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
        public ReactiveList<GistModel> Gists { get; private set; }

        public IReactiveCommand<object> GoToGistCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        protected GistsViewModel()
        {
            Gists = new ReactiveList<GistModel>();

            GoToGistCommand = ReactiveCommand.Create();
            GoToGistCommand.OfType<GistModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<GistViewModel>();
                vm.Id = x.Id;
                vm.Gist = x;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t => Gists.SimpleCollectionLoad(CreateRequest(), t as bool?));
        }

        protected abstract GitHubRequest<List<GistModel>> CreateRequest();
    }
}


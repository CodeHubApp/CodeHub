using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using GitHubSharp;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class GistsViewModel : LoadableViewModel
    {
        public ReactiveCollection<GistModel> Gists { get; private set; }

        public IReactiveCommand GoToGistCommand { get; private set; }

        protected GistsViewModel()
        {
            Gists = new ReactiveCollection<GistModel>();

            GoToGistCommand = new ReactiveCommand();
            GoToGistCommand.OfType<GistModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<GistViewModel>();
                vm.Id = x.Id;
                vm.Gist = x;
                ShowViewModel(vm);
            });

            LoadCommand.RegisterAsyncTask(t => Gists.SimpleCollectionLoad(CreateRequest(), t as bool?));
        }

        protected abstract GitHubRequest<List<GistModel>> CreateRequest();
    }
}


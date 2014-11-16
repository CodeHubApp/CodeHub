using System;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.Core;
using System.Linq;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class BaseGistsViewModel : BaseViewModel, IProvidesSearchKeyword
    {
        protected readonly ReactiveList<GistModel> GistsCollection = new ReactiveList<GistModel>();

        public IReadOnlyReactiveList<GistItemViewModel> Gists { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        protected BaseGistsViewModel()
        {
            var gotoCommand = new Action<GistModel>(x =>
            {
                var vm = CreateViewModel<GistViewModel>();
                vm.Id = x.Id;
                vm.Gist = x;
                ShowViewModel(vm);
            });

            Gists = GistsCollection.CreateDerivedCollection(
                x => CreateGistItemViewModel(x, _ => gotoCommand(x)),
                filter: x => x.Description.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));
        }

        private static GistItemViewModel CreateGistItemViewModel(GistModel gist, Action<GistItemViewModel> action)
        {
            var title = (gist.Owner == null) ? "Anonymous" : gist.Owner.Login;
            var description = string.IsNullOrEmpty(gist.Description) ? "Gist " + gist.Id : gist.Description;
            var imageUrl = (gist.Owner == null) ? null : gist.Owner.AvatarUrl;
            if (gist.Files.Count > 0)
                title = gist.Files.First().Key;
            return new GistItemViewModel(title, imageUrl, description, gist.UpdatedAt, action);
        }
    }
}


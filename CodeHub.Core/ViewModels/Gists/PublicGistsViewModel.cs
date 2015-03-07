using System;
using GitHubSharp.Models;
using ReactiveUI;
using System.Linq;
using GitHubSharp;
using System.Reactive;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Gists
{
    public class PublicGistsViewModel : BaseViewModel, ILoadableViewModel, IPaginatableViewModel
    {
        public IReadOnlyReactiveList<GistItemViewModel> Gists { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        public PublicGistsViewModel(ISessionService sessionService)
        {
            Title = "Public Gists";

            var gists = new ReactiveList<GistModel>();
            Gists = gists.CreateDerivedCollection(x => CreateGistItemViewModel(x));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                var request = sessionService.Client.Gists.GetPublicGists();
                await gists.SimpleCollectionLoad(request, t as bool?, 
                    x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x()));
            });
        }

        private GistItemViewModel CreateGistItemViewModel(GistModel gist)
        {
            var title = (gist.Owner == null) ? "Anonymous" : gist.Owner.Login;
            var description = string.IsNullOrEmpty(gist.Description) ? "Gist " + gist.Id : gist.Description;
            var imageUrl = (gist.Owner == null) ? null : gist.Owner.AvatarUrl;
            if (gist.Files.Count > 0)
                title = gist.Files.First().Key;

            return new GistItemViewModel(title, imageUrl, description, gist.UpdatedAt, _ =>
            {
                var vm = this.CreateViewModel<GistViewModel>();
                vm.Id = gist.Id;
                vm.Gist = gist;
                NavigateTo(vm);
            });
        }
    }
}

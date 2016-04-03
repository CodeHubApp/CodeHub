using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class GistsViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<GistModel> _gists = new CollectionViewModel<GistModel>();

        public CollectionViewModel<GistModel> Gists { get { return _gists; } }

        public ICommand GoToGistCommand
        {
            get { return new MvxCommand<GistModel>(x => ShowViewModel<GistViewModel>(new GistViewModel.NavObject { Id = x.Id }));}
        }

        protected override Task Load()
        {
            return Gists.SimpleCollectionLoad(CreateRequest());
        }

        protected abstract GitHubRequest<List<GistModel>> CreateRequest();
    }
}


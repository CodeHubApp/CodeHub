using System.Collections.Generic;
using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class GistsViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<GistModel> _gists = new CollectionViewModel<GistModel>();
        protected new readonly IApplicationService Application;

        protected GistsViewModel(IApplicationService application)
        {
            Application = application;
        }

        public CollectionViewModel<GistModel> Gists
        {
            get
            {
                return _gists;
            }
        }
        
        public Task Load(bool forceDataRefresh)
        {
            return Gists.SimpleCollectionLoad(CreateRequest(), forceDataRefresh);
        }

        protected abstract GitHubRequest<List<GistModel>> CreateRequest();
    }
}


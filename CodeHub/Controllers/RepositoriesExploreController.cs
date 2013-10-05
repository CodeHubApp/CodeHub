using GitHubSharp.Models;
using CodeFramework.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeHub.Controllers
{
    public class RepositoriesExploreController : ListController<RepositorySearchModel.RepositoryModel>
    {
        public bool Searched { get; private set; }

        public RepositoriesExploreController(IListView<RepositorySearchModel.RepositoryModel> view)
            : base(view)
        {
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            //Don't do anything here...
            Model = new ListModel<RepositorySearchModel.RepositoryModel> { Data = new List<RepositorySearchModel.RepositoryModel>() };
        }

        public void Search(string text)
        {
            Searched = true;
            View.ShowLoading(false, () => {
                var request = Application.Client.Repositories.SearchRepositories(text);
                request.RequestFromCache = false;

                var response = Application.Client.Execute(request);
                RenderView(new ListModel<RepositorySearchModel.RepositoryModel>(response.Data.Repositories, null));
            });
        }

    }
}


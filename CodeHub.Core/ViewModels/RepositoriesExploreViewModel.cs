using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class RepositoriesExploreViewModel : BaseViewModel
    {
        private readonly CollectionViewModel<RepositorySearchModel.RepositoryModel> _repositories = new CollectionViewModel<RepositorySearchModel.RepositoryModel>();

        public CollectionViewModel<RepositorySearchModel.RepositoryModel> Repositories
        {
            get { return _repositories; }
        }

        public bool Searched { get; private set; }

        public async Task Search(string text)
        {
            Searched = true;
            await Task.Run(() => {
                var request = Application.Client.Repositories.SearchRepositories(text);
                request.UseCache = false;
                var response = Application.Client.Execute(request);
                Repositories.Items.Reset(response.Data.Repositories);
            });
        }

    }
}


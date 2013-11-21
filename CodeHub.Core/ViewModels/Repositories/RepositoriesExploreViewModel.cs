using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class RepositoriesExploreViewModel : BaseViewModel
    {
        private readonly CollectionViewModel<RepositorySearchModel.RepositoryModel> _repositories = new CollectionViewModel<RepositorySearchModel.RepositoryModel>();
        private string _searchText;

        public bool ShowRepositoryDescription
        {
            get { return Application.Account.ShowRepositoryDescriptionInList; }
        }

        public CollectionViewModel<RepositorySearchModel.RepositoryModel> Repositories
        {
            get { return _repositories; }
        }

        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; RaisePropertyChanged(() => SearchText); }
        }

        public ICommand SearchCommand
        {
            get { return new MvxCommand(Search, () => SearchText.Length > 0);}
        }

        private async void Search()
        {
//            await Task.Run(() =>
//            {
//				var request = Application.Client.Repositories.SearchRepositories(SearchText);
//                request.UseCache = false;
//                var response = Application.Client.Execute(request);
//                Repositories.Items.Reset(response.Data.Repositories);
//            });
        }
    }
}


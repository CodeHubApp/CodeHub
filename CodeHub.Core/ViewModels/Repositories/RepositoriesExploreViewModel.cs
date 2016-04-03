using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Repositories;
using System;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesExploreViewModel : BaseViewModel
    {
        private readonly CollectionViewModel<RepositorySearchModel.RepositoryModel> _repositories = new CollectionViewModel<RepositorySearchModel.RepositoryModel>();
        private string _searchText;

        public bool ShowRepositoryDescription
        {
            get { return this.GetApplication().Account.ShowRepositoryDescriptionInList; }
        }

        public CollectionViewModel<RepositorySearchModel.RepositoryModel> Repositories
        {
            get { return _repositories; }
        }

        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            private set { this.RaiseAndSetIfChanged(ref _isSearching, value); }
        }

        public ICommand GoToRepositoryCommand
        {
            get { return new MvxCommand<RepositorySearchModel.RepositoryModel>(x => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner.Login, Repository = x.Name })); }
        }

        public ICommand SearchCommand
        {
            get { return new MvxCommand(() => Search(), () => !string.IsNullOrEmpty(SearchText)); }
        }

        private async Task Search()
        {
            try
            {
                IsSearching = true;

                var request = this.GetApplication().Client.Repositories.SearchRepositories(new [] { SearchText }, new string[] { });
                var response = await this.GetApplication().Client.ExecuteAsync(request);
                Repositories.Items.Reset(response.Data.Items);
            }
            catch
            {
                DisplayAlert("Unable to search for repositories. Please try again.");
            }
            finally
            {
                IsSearching = false;
            }
        }
    }
}


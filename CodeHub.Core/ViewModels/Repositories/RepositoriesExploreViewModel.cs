using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Repositories;
using System;

namespace CodeHub.Core.ViewModels
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
            set { _searchText = value; RaisePropertyChanged(() => SearchText); }
        }

		private bool _isSearching;
		public bool IsSearching
		{
			get { return _isSearching; }
			private set
			{
				_isSearching = value;
				RaisePropertyChanged(() => IsSearching);
			}
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
                request.UseCache = false;
				var response = await this.GetApplication().Client.ExecuteAsync(request);
				Repositories.Items.Reset(response.Data.Items);
			}
			catch (Exception e)
			{
				ReportError(e);
			}
			finally
			{
				IsSearching = false;
			}
        }
    }
}


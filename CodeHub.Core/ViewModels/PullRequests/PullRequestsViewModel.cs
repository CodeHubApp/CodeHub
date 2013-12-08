using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel : LoadableViewModel
    {
		private readonly CollectionViewModel<PullRequestModel> _pullrequests = new CollectionViewModel<PullRequestModel>();
		public CollectionViewModel<PullRequestModel> PullRequests
        {
            get { return _pullrequests; }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

		private int _selectedFilter;
		public int SelectedFilter
		{
			get { return _selectedFilter; }
			set 
			{
				_selectedFilter = value;
				RaisePropertyChanged(() => SelectedFilter);
			}
		}

        public ICommand GoToPullRequestCommand
        {
            get { return new MvxCommand<PullRequestModel>(x => ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = Username, Repository = Repository, Id = x.Number })); }
        }

		public PullRequestsViewModel()
		{
			this.Bind(x => x.SelectedFilter, () => LoadCommand.Execute(null));
		}

		public void Init(NavObject navObject) 
        {
			Username = navObject.Username;
			Repository = navObject.Repository;
        }

        protected override Task Load(bool forceCacheInvalidation)
        {
			var state = SelectedFilter == 0 ? "open" : "closed";
			var request = this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests.GetAll(state: state);
            return PullRequests.SimpleCollectionLoad(request, forceCacheInvalidation);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

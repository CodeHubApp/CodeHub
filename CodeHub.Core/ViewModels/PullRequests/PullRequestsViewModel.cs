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
		private bool _isShowingOpened;

		public CollectionViewModel<PullRequestModel> PullRequests
        {
            get { return _pullrequests; }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

		public bool IsShowingOpened
		{
			get { return _isShowingOpened; }
			set
			{
				_isShowingOpened = value;
				RaisePropertyChanged(() => IsShowingOpened);
			}
		}

        public ICommand GoToPullRequestCommand
        {
            get { return new MvxCommand<PullRequestModel>(x => ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = Username, Repository = Repository, Id = x.Number })); }
        }

		public ICommand ShowOpenedCommand
		{
			get { return new MvxCommand(ShowOpened, () => !IsShowingOpened); }
		}

		public ICommand ShowClosedCommand
		{
			get { return new MvxCommand(ShowClosed, () => IsShowingOpened); }
		}

		public PullRequestsViewModel()
		{
			IsShowingOpened = true;
		}

		public void Init(NavObject navObject) 
        {
			Username = navObject.Username;
			Repository = navObject.Repository;
        }

		private void ShowOpened()
		{
			IsShowingOpened = true;
			LoadCommand.Execute(null);
		}

		private void ShowClosed()
		{
			IsShowingOpened = false;
			LoadCommand.Execute(null);
		}

        protected override Task Load(bool forceDataRefresh)
        {
			var state = IsShowingOpened ? "open" : "closed";
			var request = this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests.GetAll(state: state);
            return PullRequests.SimpleCollectionLoad(request, forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

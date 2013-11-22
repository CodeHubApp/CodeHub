using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Accounts;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.User;
using System.Linq;

namespace CodeHub.Core.ViewModels.App
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IApplicationService _application;
        private static readonly IDictionary<string, string> Presentation = new Dictionary<string, string> {{PresentationValues.SlideoutRootPresentation, string.Empty}};  
        private int _notifications;
		private List<string> _organizations;

        public int Notifications
        {
            get { return _notifications; }
            set { _notifications = value; RaisePropertyChanged(() => Notifications); }
        }

		public List<string> Organizations
        {
            get { return _organizations; }
            set { _organizations = value; RaisePropertyChanged(() => Organizations); }
        }

        public void Init()
        {
            GoToDefaultTopView.Execute(null);
        }

        public GitHubAccount Account
        {
            get { return _application.Account; }
        }

		public IEnumerable<CodeFramework.Core.Data.PinnedRepository> PinnedRepositories
		{
			get { return _application.Account.PinnnedRepositories; }
		}

        public MenuViewModel(IApplicationService application)
        {
            _application = application;
        }

        public ICommand GoToAccountsCommand
        {
            get { return new MvxCommand(() => this.ShowViewModel<AccountsViewModel>()); }
        }

        public ICommand GoToProfileCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = _application.Account.Username })); }
        }

        public ICommand GoToNotificationsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<NotificationsViewModel>(null)); }
        }

        public ICommand GoToMyIssuesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<MyIssuesViewModel>(null)); }
        }

        public ICommand GoToMyEvents
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserEventsViewModel>(new UserEventsViewModel.NavObject { Username = Account.Username })); }
        }

        public ICommand GoToMyGistsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserGistsViewModel>(new UserGistsViewModel.NavObject { Username = Account.Username}));}
        }

        public ICommand GoToStarredGistsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<StarredGistsViewModel>(null)); }
        }

        public ICommand GoToPublicGistsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<PublicGistsViewModel>(null)); }
        }

        public ICommand GoToDefaultTopView
        {
            get { return GoToProfileCommand; }
        }

        public ICommand GoToStarredRepositoriesCommand
        {
			get { return new MvxCommand(() => ShowMenuViewModel<RepositoriesStarredViewModel>(null));}
        }

		public ICommand GoToOwnedRepositoriesCommand
		{
			get { return new MvxCommand(() => ShowMenuViewModel<UserRepositoriesViewModel>(new UserRepositoriesViewModel.NavObject { Username = Account.Username }));}
		}

		public ICommand GoToExploreRepositoriesCommand
		{
			get { return new MvxCommand(() => ShowMenuViewModel<RepositoriesExploreViewModel>(null));}
		}

		public ICommand GoToOrganizationEventsCommand
		{
			get { return new MvxCommand<string>(x => ShowMenuViewModel<Events.UserEventsViewModel>(new Events.UserEventsViewModel.NavObject { Username = x }));}
		}

		public ICommand GoToOrganizationCommand
		{
			get { return new MvxCommand<string>(x => ShowMenuViewModel<Organizations.OrganizationViewModel>(new Organizations.OrganizationViewModel.NavObject { Name = x }));}
		}

		public ICommand GoToOrganizationsCommand
		{
			get { return new MvxCommand(() => ShowMenuViewModel<Organizations.OrganizationsViewModel>(new Organizations.OrganizationsViewModel.NavObject { Username = Account.Username }));}
		}

        public ICommand GoToNewsComamnd
        {
            get { return new MvxCommand(() => ShowMenuViewModel<NewsViewModel>(null));}
        }

		public ICommand GoToRepositoryComamnd
		{
			get { return new MvxCommand<Utils.RepositoryIdentifier>(x => ShowMenuViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner, Repository = x.Name }));}
		}

        public ICommand LoadCommand
        {
            get { return new MvxCommand(Load);}    
        }

        private bool ShowMenuViewModel<T>(object data) where T : IMvxViewModel
        {
            return this.ShowViewModel<T>(data, new MvxBundle(Presentation));
        }

        private async void Load()
        {
			var t1 = Application.Client.ExecuteAsync(Application.Client.Notifications.GetAll()).ContinueWith(x =>
			{
				Notifications = x.Result.Data.Count;
			}, TaskContinuationOptions.OnlyOnRanToCompletion);

			var t2 = Application.Client.ExecuteAsync(Application.Client.AuthenticatedUser.GetOrganizations()).ContinueWith(x =>
			{
				Organizations = x.Result.Data.Select(y => y.Login).ToList();
			},TaskContinuationOptions.OnlyOnRanToCompletion);

			await Task.WhenAll(t1, t2);
        }
    }
}

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
            get { return new MvxCommand(() => this.ShowMenuViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = _application.Account.Username })); }
        }

        public ICommand GoToNotificationsCommand
        {
            get { return new MvxCommand(() => this.ShowMenuViewModel<NotificationsViewModel>(null)); }
        }

        public ICommand GoToMyIssuesCommand
        {
            get { return new MvxCommand(() => this.ShowMenuViewModel<MyIssuesViewModel>(null)); }
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
            get { return new MvxCommand(() => this.ShowViewModel<RepositoriesStarredViewModel>());}
        }

        public ICommand GoToNewsComamnd
        {
            get { return new MvxCommand(() => ShowMenuViewModel<NewsViewModel>(null));}
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
            var notifications = await Application.Client.ExecuteAsync(Application.Client.Notifications.GetAll());
            Notifications = notifications.Data.Count;
        }
    }
}

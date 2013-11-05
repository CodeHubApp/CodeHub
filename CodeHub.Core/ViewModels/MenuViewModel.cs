using System;
using System.Collections.Generic;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core;
using CodeHub.Core.Data;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels
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

        public ICommand GoToDefaultTopView
        {
            get { return GoToProfileCommand; }
        }

        private bool ShowMenuViewModel<T>(object data) where T : IMvxViewModel
        {
            return this.ShowViewModel<T>(data, new MvxBundle(Presentation));
        }
    }
}

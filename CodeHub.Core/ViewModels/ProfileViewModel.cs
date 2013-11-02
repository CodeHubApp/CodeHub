using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels
{
    public class ProfileViewModel : BaseViewModel, ILoadableViewModel
    {
        private UserModel _userModel;
        private readonly IApplicationService _application;

        public string Username
        {
            get;
            private set;
        }

        public UserModel User
        {
            get { return _userModel; }
            private set
            {
                _userModel = value;
                RaisePropertyChanged(() => User);
            }
        }

        public ProfileViewModel(IApplicationService application)
        {
            _application = application;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        public Task Load(bool forceDataRefresh)
        {
            return Task.Run(() => this.RequestModel(_application.Client.Users[Username].Get(), forceDataRefresh, response => User = response.Data));
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}


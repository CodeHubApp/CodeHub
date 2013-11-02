using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels
{
    public class OrganizationViewModel : BaseViewModel, ILoadableViewModel
	{
        private UserModel _userModel;

        public string Name 
        { 
            get; 
            private set; 
        }

		public void Init(NavObject navObject) 
		{
			Name = navObject.Name;
		}

        public UserModel Organization
        {
            get { return _userModel; }
            private set
            {
                _userModel = value;
                RaisePropertyChanged(() => Organization);
            }
        }

        public Task Load(bool forceDataRefresh)
        {
            return Task.Run(() => this.RequestModel(Application.Client.Organizations[Name].Get(), forceDataRefresh, response => Organization = response.Data));
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
	}
}


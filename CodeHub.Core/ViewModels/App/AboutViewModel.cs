using CodeFramework.Core.ViewModels;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.ViewModels.Repositories;
using CodeFramework.Core.Services;

namespace CodeHub.Core.ViewModels.App
{
	public class AboutViewModel : BaseViewModel
    {
        private readonly IEnvironmentService _environmentService;

        public AboutViewModel(IEnvironmentService environmentService)
        {
            _environmentService = environmentService;
        }

        public string Version
        {
            get { return _environmentService.ApplicationVersion; }
        }

        public ICommand GoToSourceCodeCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Repository = "codehub", Username = "thedillonb" })); }
        }
    }
}


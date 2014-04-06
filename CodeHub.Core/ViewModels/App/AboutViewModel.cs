using CodeFramework.Core.ViewModels;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.Core.ViewModels.App
{
	public class AboutViewModel : BaseViewModel
    {
        public ICommand GoToSourceCodeCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Repository = "codehub", Username = "thedillonb" })); }
        }
    }
}


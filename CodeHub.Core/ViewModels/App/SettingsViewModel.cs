using CodeFramework.Core.ViewModels;
using System.Windows.Input;
using CodeFramework.Core.ViewModels.App;
using Cirrious.MvvmCross.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
    public class SettingsViewModel : BaseViewModel
    {
		public string DefaultStartupViewName
		{
			get { return this.GetApplication().Account.DefaultStartupView; }
		}

		public ICommand GoToDefaultStartupViewCommand
		{
			get { return new MvxCommand(() => ShowViewModel<DefaultStartupViewModel>()); }
		}
    }
}

using System.Windows.Input;
using CodeHub.Core.ViewModels;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class NewAccountViewModel : BaseViewModel
    {
        public ICommand GoToDotComLoginCommand
        {
			get { return new MvxCommand(() => this.ShowViewModel<LoginViewModel>(new LoginViewModel.NavObject())); }
        }

        public ICommand GoToEnterpriseLoginCommand
        {
            get { return new MvxCommand(() => this.ShowViewModel<AddAccountViewModel>(new AddAccountViewModel.NavObject())); }
        }
    }
}

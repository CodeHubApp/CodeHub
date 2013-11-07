using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;

namespace CodeHub.Core.ViewModels
{
    public class NewAccountViewModel : BaseViewModel
    {
        public ICommand GoToDotComLoginCommand
        {
            get { return new MvxCommand(() => this.ShowViewModel<AddAccountViewModel>(new AddAccountViewModel.NavObject { IsEnterprise = false })); }
        }

        public ICommand GoToEnterpriseLoginCommand
        {
            get { return new MvxCommand(() => this.ShowViewModel<AddAccountViewModel>(new AddAccountViewModel.NavObject { IsEnterprise = true })); }
        }
    }
}

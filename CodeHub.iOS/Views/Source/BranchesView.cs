using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Source
{
    public class BranchesView : ViewModelCollectionDrivenViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Branches".t();
            NoItemsText = "No Branches".t();

            base.ViewDidLoad();

            var vm = (BranchesViewModel) ViewModel;
            this.BindCollection(vm.Branches, x => new StyledStringElement(x.Name, () => vm.GoToSourceCommand.Execute(x)));
        }
    }
}

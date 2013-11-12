using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Organizations;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationsView : ViewModelCollectionDrivenViewController
	{
        public override void ViewDidLoad()
        {
            Title = "Organizations".t();
            SearchPlaceholder = "Search Organizations".t();
            NoItemsText = "No Organizations".t();

            base.ViewDidLoad();

            var vm = (OrganizationsViewModel) ViewModel;
            BindCollection(vm.Organizations, x => new StyledStringElement(x.Login, () => vm.GoToOrganizationCommand.Execute(x)));
        }
	}
}


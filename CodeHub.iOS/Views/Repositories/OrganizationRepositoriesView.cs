using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.iOS.Views.Repositories
{
    public class OrganizationRepositoriesView : BaseRepositoriesView
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (OrganizationRepositoriesViewModel) ViewModel;
            Title = vm.Name;
        }
    }
}


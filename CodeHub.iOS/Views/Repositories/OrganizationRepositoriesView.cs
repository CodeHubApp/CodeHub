using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.iOS.Views.Repositories
{
	public class OrganizationRepositoriesView : BaseRepositoriesView<OrganizationRepositoriesViewModel>
	{
	    public override void ViewDidLoad()
	    {
	        Title = ViewModel.Name;
	        base.ViewDidLoad();
	    }
	}
}


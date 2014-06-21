using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.iOS.Views.Repositories
{
	public class RepositoriesStarredView : BaseRepositoriesView<RepositoriesStarredViewModel>
    {
	    public override void LoadView()
	    {
	        Title = "Starred";
	        base.LoadView();
	    }
    }
}


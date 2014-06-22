using CodeHub.iOS.Views.User;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.iOS.Views.Repositories
{
    public class StargazersView : BaseUserCollectionView<StargazersViewModel>
    {
		public override void ViewDidLoad()
		{
			Title = "Stargazers";
			NoItemsText = "No Stargazers";
			base.ViewDidLoad();
		}
    }
}


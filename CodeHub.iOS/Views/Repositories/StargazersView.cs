using CodeHub.iOS.Views.User;

namespace CodeHub.iOS.Views.Repositories
{
    public class StargazersView : BaseUserCollectionView
    {
		public override void ViewDidLoad()
		{
			Title = "Stargazers";
			NoItemsText = "No Stargazers";
			base.ViewDidLoad();
		}
    }

    public class WatchersView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Watchers";
            NoItemsText = "No Watchers";
            base.ViewDidLoad();
        }
    }
}


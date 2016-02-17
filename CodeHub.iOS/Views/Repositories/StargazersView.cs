using CodeHub.iOS.Views.User;

namespace CodeHub.iOS.Views.Repositories
{
    public class StargazersView : BaseUserCollectionView
    {
        public StargazersView()
		{
			Title = "Stargazers";
			NoItemsText = "No Stargazers";
		}
    }

    public class WatchersView : BaseUserCollectionView
    {
        public WatchersView()
        {
            Title = "Watchers";
            NoItemsText = "No Watchers";
        }
    }
}


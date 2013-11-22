using CodeFramework.iOS.Elements;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Views.User;

namespace CodeHub.iOS.Views.Repositories
{
    public class StargazersView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Stargazers".t();
            NoItemsText = "No Stargazers".t();

            base.ViewDidLoad();
        }
    }
}


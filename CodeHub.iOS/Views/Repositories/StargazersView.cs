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
            SearchPlaceholder = "Search Stargazers".t();
            NoItemsText = "No Stargazers".t();

            base.ViewDidLoad();

            var vm = (StargazersViewModel) ViewModel;
            BindCollection(vm.Stargazers, x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Tapped += () => vm.GoToUserCommand.Execute(x);
                return e;
            });
        }
    }
}


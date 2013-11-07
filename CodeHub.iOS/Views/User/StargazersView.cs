using CodeFramework.iOS.Elements;
using CodeHub.Core.ViewModels;

namespace CodeHub.iOS.Views.User
{
    public class StargazersView : BaseFollowersView
    {
        public new StargazersViewModel ViewModel
        {
            get { return (StargazersViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            Title = "Stargazers".t();
            SearchPlaceholder = "Search Stargazers".t();
            NoItemsText = "No Stargazers".t();
            base.ViewDidLoad();

            BindCollection(ViewModel.Stargazers, x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Tapped += () => ViewModel.GoToUserCommand.Execute(x);
                return e;
            });

        }
    }
}


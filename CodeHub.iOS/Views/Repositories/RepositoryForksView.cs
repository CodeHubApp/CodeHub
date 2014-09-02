using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoryForksView : BaseRepositoriesView<RepositoryForksViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Forks";
        }
    }
}


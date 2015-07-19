using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Views;
using UIKit;
using System;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoryForksViewController : BaseRepositoriesView<RepositoryForksViewModel>
    {
        public RepositoryForksViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.RepoForked.ToEmptyListImage(), "There are no forks."));
        }
    }
}


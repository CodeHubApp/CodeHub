using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using System;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoryForksView : BaseRepositoriesView<RepositoryForksViewModel>
    {
        public RepositoryForksView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.RepoForked.ToEmptyListImage(), "There are no forks."));
        }

    }
}


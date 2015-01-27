using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using System;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoryForksView : BaseRepositoriesView<RepositoryForksViewModel>
    {
        protected RepositoryForksView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.RepoForked.ToImage(64f), "There are no forks."));
        }

    }
}


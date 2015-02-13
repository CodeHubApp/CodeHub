using CodeHub.Core.ViewModels.Repositories;
using System;
using UIKit;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Repositories
{
	public class RepositoriesStarredView : BaseRepositoriesView<RepositoriesStarredViewModel>
    {
        public RepositoriesStarredView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Star.ToEmptyListImage(), "You have not starred any repositories."));
        }
    }
}


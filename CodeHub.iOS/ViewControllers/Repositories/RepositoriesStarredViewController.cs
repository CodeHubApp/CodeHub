using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Views;
using System;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Repositories
{
	public class RepositoriesStarredViewController : BaseRepositoriesView<RepositoriesStarredViewModel>
    {
        public RepositoriesStarredViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Star.ToEmptyListImage(), "You have not starred any repositories."));
        }
    }
}


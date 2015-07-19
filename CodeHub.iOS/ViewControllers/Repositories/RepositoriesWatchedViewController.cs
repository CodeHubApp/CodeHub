using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Views;
using System;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Repositories
{
	public class RepositoriesWatchedViewController : BaseRepositoriesView<RepositoriesWatchedViewModel>
    {
        public RepositoriesWatchedViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Eye.ToEmptyListImage(), "You are not watching any repositories."));
        }
    }
}


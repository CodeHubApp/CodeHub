using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.ViewComponents;
using System;
using UIKit;

namespace CodeHub.iOS.Views.Repositories
{
	public class RepositoriesWatchedView : BaseRepositoriesView<RepositoriesWatchedViewModel>
    {
        public RepositoriesWatchedView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Eye.ToImage(64f), "You are not watching any repositories."));
        }
    }
}


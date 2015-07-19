using CodeHub.Core.ViewModels.Gists;
using System;
using UIKit;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class StarredGistsViewController : BaseGistsViewController<StarredGistsViewModel>
    {
        public StarredGistsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Star.ToEmptyListImage(), "You have not starred any gists."));
        }
    }
}
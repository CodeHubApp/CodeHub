using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.ViewComponents;
using System;
using UIKit;

namespace CodeHub.iOS.Views.Gists
{
    public class StarredGistsView : BaseGistsView<StarredGistsViewModel>
    {
        public StarredGistsView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Star.ToEmptyListImage(), "You have not starred any gists."));
        }
    }
}
using System;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewComponents;
using UIKit;

namespace CodeHub.iOS.Views.Gists
{
    public class PublicGistsView : BaseTableViewController<PublicGistsViewModel>
    {
        public PublicGistsView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Gist.ToEmptyListImage(), "There are no gists."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new GistTableViewSource(TableView, ViewModel.Gists);
        }
    }
}
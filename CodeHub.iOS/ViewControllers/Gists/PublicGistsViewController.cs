using System;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class PublicGistsViewController : BaseTableViewController<PublicGistsViewModel>
    {
        public PublicGistsViewController()
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
using CodeHub.Core.ViewModels.Releases;
using CodeHub.iOS.Views;
using CodeHub.iOS.TableViewSources;
using System;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Releases
{
    public class ReleasesViewController : BaseTableViewController<ReleasesViewModel> 
    {
        public ReleasesViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Tag.ToEmptyListImage(), "There are no releases."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new ReleasesTableViewSource(TableView, ViewModel.Items);
        }
    }
}


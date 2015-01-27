using CodeHub.Core.ViewModels.Releases;
using CodeHub.iOS.TableViewSources;
using System;
using UIKit;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Releases
{
    public class ReleasesView : BaseTableViewController<ReleasesViewModel> 
    {
        public ReleasesView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Tag.ToImage(64f), "There are no releases."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new ReleasesTableViewSource(TableView, ViewModel.Releases);
        }
    }
}


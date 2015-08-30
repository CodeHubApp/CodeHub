using CodeHub.Core.ViewModels.Releases;
using CodeHub.iOS.Views;
using CodeHub.iOS.TableViewSources;
using System;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Releases
{
    public class ReleasesViewController : BaseTableViewController<ReleasesViewModel> 
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Tag.ToEmptyListImage(), "There are no releases."));

            this.WhenAnyValue(x => x.ViewModel.Items)
                .Select(x => new ReleasesTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}


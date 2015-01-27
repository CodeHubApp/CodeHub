using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.TableViewSources;
using UIKit;
using CodeHub.iOS.ViewComponents;
using System;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class BaseGistsView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseGistsViewModel
    {
        protected BaseGistsView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Gist.ToImage(64f), "There are no gists."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new GistTableViewSource(TableView, ViewModel.Gists);
        }
    }
}


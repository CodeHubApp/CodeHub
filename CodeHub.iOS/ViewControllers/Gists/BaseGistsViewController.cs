using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using UIKit;
using System;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public abstract class BaseGistsViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseGistsViewModel
    {
        protected BaseGistsViewController()
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


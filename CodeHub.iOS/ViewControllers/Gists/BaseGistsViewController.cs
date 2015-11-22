using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using UIKit;
using System;
using CodeHub.Core.ViewModels;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public abstract class BaseGistsViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : class, IListViewModel<GistItemViewModel>
    {
        protected BaseGistsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Gist.ToEmptyListImage(), "There are no gists."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new GistTableViewSource(TableView, ViewModel.Items);
        }
    }
}


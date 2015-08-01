using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using UIKit;
using System;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public abstract class BaseGistsViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : class, IGistsViewModel
    {
        protected BaseGistsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Gist.ToEmptyListImage(), "There are no gists."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.Gists)
                .Select(x => new GistTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}


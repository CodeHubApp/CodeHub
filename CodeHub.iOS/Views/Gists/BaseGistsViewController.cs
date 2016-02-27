using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Gists;
using UIKit;
using CodeHub.iOS.DialogElements;
using System;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class BaseGistsViewController : ViewModelCollectionDrivenDialogViewController
    {
        protected BaseGistsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Gist.ToEmptyListImage(), "There are no gists."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.EstimatedRowHeight = 64f;
            TableView.RowHeight = UITableView.AutomaticDimension;

            var vm = (GistsViewModel) ViewModel;
            var weakVm = new WeakReference<GistsViewModel>(vm);
            BindCollection(vm.Gists, x => new GistElement(x, () => weakVm.Get()?.GoToGistCommand.Execute(x)));
        }
    }
}


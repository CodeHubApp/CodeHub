using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.Elements;
using UIKit;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class GistsView : ViewModelCollectionDrivenDialogViewController
    {
        public override void ViewDidLoad()
        {
            NoItemsText = "No Gists";

            base.ViewDidLoad();

            TableView.EstimatedRowHeight = 64f;
            TableView.RowHeight = UITableView.AutomaticDimension;

            var vm = (GistsViewModel) ViewModel;
            BindCollection(vm.Gists, x => new GistElement(x, () => vm.GoToGistCommand.Execute(x)));
        }
    }
}


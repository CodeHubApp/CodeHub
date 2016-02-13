using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Changesets;
using CodeHub.iOS.Elements;
using UIKit;

namespace CodeHub.iOS.Views.Source
{
	public abstract class CommitsView : ViewModelCollectionDrivenDialogViewController
	{
		public override void ViewDidLoad()
		{
			Title = "Commits";

			base.ViewDidLoad();

            TableView.EstimatedRowHeight = 64f;
            TableView.RowHeight = UITableView.AutomaticDimension;

			var vm = (CommitsViewModel) ViewModel;
			BindCollection(vm.Commits, x => new CommitElement(x, () => vm.GoToChangesetCommand.Execute(x)));
		}
	}
}


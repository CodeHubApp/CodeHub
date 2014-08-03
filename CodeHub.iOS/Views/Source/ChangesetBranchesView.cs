using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetBranchesView : ViewModelCollectionViewController<ChangesetBranchesViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Changeset Branch";
            //NoItemsText = "No Branches";

            base.ViewDidLoad();

            this.BindList(ViewModel.Branches, x => new StyledStringElement(x.Name, () => ViewModel.GoToBranchCommand.Execute(x)));
        }
    }
}


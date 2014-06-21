using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.Dialog;
using ReactiveUI;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetBranchesView : ViewModelCollectionView<ChangesetBranchesViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Changeset Branch";
            NoItemsText = "No Branches";

            base.ViewDidLoad();

            Bind(ViewModel.WhenAnyValue(x => x.Branches), x => new StyledStringElement(x.Name, () => ViewModel.GoToBranchCommand.Execute(x)));
        }
    }
}


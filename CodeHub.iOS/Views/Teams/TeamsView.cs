using CodeFramework.iOS.Views;
using MonoTouch.Dialog;
using CodeHub.Core.ViewModels.Teams;
using ReactiveUI;

namespace CodeHub.iOS.Views.Teams
{
    public class TeamsView : ViewModelCollectionView<TeamsViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Teams";
            NoItemsText = "No Teams";

            base.ViewDidLoad();

            Bind(ViewModel.WhenAnyValue(x => x.Teams), 
                x => new StyledStringElement(x.Name, () => ViewModel.GoToTeamCommand.Execute(x)));
        }
    }
}
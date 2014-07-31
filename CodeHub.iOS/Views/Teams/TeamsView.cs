using CodeHub.Core.ViewModels.Teams;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Teams
{
    public class TeamsView : ViewModelCollectionViewController<TeamsViewModel>
    {
        public TeamsView()
        {
            Title = "Teams";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.BindList(ViewModel.Teams, 
                x => new StyledStringElement(x.Name, () => ViewModel.GoToTeamCommand.Execute(x)));
        }
    }
}
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.User
{
    public class TeamsView : ViewModelCollectionDrivenViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Teams".t();
            SearchPlaceholder = "Search Teams".t();
            NoItemsText = "No Teams".t();

            base.ViewDidLoad();

            var vm = (TeamsViewModel) ViewModel;
            this.BindCollection(vm.Teams, x => new StyledStringElement(x.Name, () => vm.GoToTeamCommand.Execute(x)));
        }
    }
}
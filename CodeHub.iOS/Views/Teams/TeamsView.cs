using CodeHub.iOS.ViewControllers;
using MonoTouch.Dialog;
using CodeHub.Core.ViewModels.Teams;
using System;

namespace CodeHub.iOS.Views.Teams
{
    public class TeamsView : ViewModelCollectionDrivenDialogViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Teams".t();
            NoItemsText = "No Teams".t();

            base.ViewDidLoad();

            var vm = (TeamsViewModel) ViewModel;
            this.BindCollection(vm.Teams, x => new StyledStringElement(x.Name, () => vm.GoToTeamCommand.Execute(x)));
        }
    }
}
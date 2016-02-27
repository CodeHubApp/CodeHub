using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Teams;
using CodeHub.iOS.DialogElements;
using System;
using UIKit;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Teams
{
    public class TeamsViewController : ViewModelCollectionDrivenDialogViewController
    {
        public TeamsViewController()
        {
            Title = "Teams";
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Organization.ToEmptyListImage(), "There are no teams."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (TeamsViewModel) ViewModel;
            var weakVm = new WeakReference<TeamsViewModel>(vm);
            this.BindCollection(vm.Teams, x => {
                var e = new StringElement(x.Name);
                e.Clicked.Subscribe(_ => weakVm.Get()?.GoToTeamCommand.Execute(x));
                return e;
            });
        }
    }
}
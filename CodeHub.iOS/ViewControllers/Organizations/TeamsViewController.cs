using System;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Organizations
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
using System;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers.Users;
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

            var vm = (TeamsViewModel)ViewModel;
            var weakVm = new WeakReference<TeamsViewController>(this);

            this.BindCollection(vm.Teams, x =>
            {
                var e = new StringElement(x.Name);
                e.Clicked.Subscribe(_ => {
                    var vc = UsersViewController.CreateTeamMembersViewController((int)x.Id);
                    weakVm.Get()?.NavigationController.PushViewController(vc, true);
                });
                return e;
            });
        }
    }
}
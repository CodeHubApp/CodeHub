using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.iOS.DialogElements;
using System;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationsView : ViewModelCollectionDrivenDialogViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Organizations";
            NoItemsText = "No Organizations";

            base.ViewDidLoad();

            var vm = (OrganizationsViewModel) ViewModel;
            var weakVm = new WeakReference<OrganizationsViewModel>(vm);
            BindCollection(vm.Organizations, x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Clicked.Subscribe(_ => weakVm.Get()?.GoToOrganizationCommand.Execute(x));
                return e;
            });
        }
    }
}


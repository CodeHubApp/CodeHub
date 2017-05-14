using CodeHub.Core.ViewModels.Organizations;
using CodeHub.iOS.DialogElements;
using System;
using UIKit;
using CodeHub.iOS.Views;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class OrganizationsViewController : ViewModelCollectionDrivenDialogViewController
    {
        public OrganizationsViewController()
        {
            Title = "Organizations";
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Organization.ToEmptyListImage(), "There are no organizations."));
        }

        public OrganizationsViewController(string username) : this()
        {
            var viewModel = new OrganizationsViewModel();
            viewModel.Init(new OrganizationsViewModel.NavObject { Username = username });
            ViewModel = viewModel;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (OrganizationsViewModel) ViewModel;
            var weakVm = new WeakReference<OrganizationsViewModel>(vm);
            BindCollection(vm.Organizations, x =>
            {
                var avatar = new GitHubAvatar(x.AvatarUrl);
                var e = new UserElement(x.Login, string.Empty, string.Empty, avatar);
                e.Clicked.Subscribe(_ => weakVm.Get()?.GoToOrganizationCommand.Execute(x));
                return e;
            });
        }
    }
}


using System;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.User;
using UIKit;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Users
{
    public abstract class BaseUserCollectionViewController : ViewModelCollectionDrivenDialogViewController
    {
        protected BaseUserCollectionViewController(string emptyString)
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Person.ToEmptyListImage(), emptyString ?? "There are no users."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (BaseUserCollectionViewModel)ViewModel;
            var weakVm = new WeakReference<BaseUserCollectionViewModel>(vm);
            BindCollection(vm.Users, x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Clicked.Subscribe(_ => weakVm.Get()?.GoToUserCommand.Execute(x));
                return e;
            });
        }
    }
}


using System;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.User;

namespace CodeHub.iOS.Views.User
{
    public abstract class BaseUserCollectionView : ViewModelCollectionDrivenDialogViewController
    {
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


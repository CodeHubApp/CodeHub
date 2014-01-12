using CodeFramework.iOS.Elements;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.User;

namespace CodeHub.iOS.Views.User
{
    public abstract class BaseUserCollectionView : ViewModelCollectionDrivenDialogViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (BaseUserCollectionViewModel)ViewModel;
            BindCollection(vm.Users, x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Tapped += () => vm.GoToUserCommand.Execute(x);
                return e;
            });
        }
    }
}


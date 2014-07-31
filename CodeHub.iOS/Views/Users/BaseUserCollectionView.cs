using CodeFramework.iOS.Elements;
using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Users
{
    public abstract class BaseUserCollectionView<TViewModel> : ViewModelCollectionViewController<TViewModel> where TViewModel : BaseUserCollectionViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.BindList(ViewModel.Users, x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Tapped += () => ViewModel.GoToUserCommand.Execute(x);
                return e;
            });
        }
    }
}


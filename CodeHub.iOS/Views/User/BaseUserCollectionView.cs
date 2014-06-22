using CodeFramework.iOS.Elements;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.User;
using ReactiveUI;

namespace CodeHub.iOS.Views.User
{
    public abstract class BaseUserCollectionView<TViewModel> : ViewModelCollectionView<TViewModel> where TViewModel : BaseUserCollectionViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Bind(ViewModel.WhenAnyValue(x => x.Users), x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Tapped += () => ViewModel.GoToUserCommand.Execute(x);
                return e;
            });
        }
    }
}


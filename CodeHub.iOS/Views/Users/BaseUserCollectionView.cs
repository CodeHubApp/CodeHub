using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using System;
using CodeHub.iOS.Elements;

namespace CodeHub.iOS.Views.Users
{
    public abstract class BaseUserCollectionView<TViewModel> : ViewModelCollectionViewController<TViewModel> where TViewModel : BaseUserCollectionViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SearchTextChanging.Subscribe(x => ViewModel.SearchKeyword = x);

            this.BindList(ViewModel.Users, x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Tapped += () => ViewModel.GoToUserCommand.Execute(x);
                return e;
            });
        }
    }
}


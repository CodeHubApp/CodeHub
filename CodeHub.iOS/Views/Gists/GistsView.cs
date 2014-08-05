using System;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using CodeHub.iOS.Elements;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class GistsView<TViewModel> : ViewModelCollectionViewController<TViewModel> where TViewModel : GistsViewModel
    {
        protected GistsView(bool searchbarEnabled)
            : base(unevenRows: true, searchbarEnabled: searchbarEnabled)
        {
            this.WhenActivated(d =>
            {
                d(SearchTextChanging.Subscribe(x => ViewModel.SearchKeyword = x));
            });
        }

        public override void ViewDidLoad()
        {
            //NoItemsText = "No Gists";

            base.ViewDidLoad();

            this.BindList(ViewModel.Gists, x => new GistElement(
                (x.Owner == null) ? "Anonymous" : x.Owner.Login,
                string.IsNullOrEmpty(x.Description) ? "Gist " + x.Id : x.Description,
                x.UpdatedAt.ToDaysAgo(),
                (x.Owner == null) ? null : x.Owner.AvatarUrl,
                () => ViewModel.GoToGistCommand.Execute(x)));
        }
    }
}


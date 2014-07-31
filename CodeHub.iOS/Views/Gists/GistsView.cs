using System;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class GistsView<TViewModel> : ViewModelCollectionViewController<TViewModel> where TViewModel : GistsViewModel
    {
        public override void ViewDidLoad()
        {
            //NoItemsText = "No Gists";

            base.ViewDidLoad();

            this.BindList(ViewModel.Gists, x =>
            {
                var str = string.IsNullOrEmpty(x.Description) ? "Gist " + x.Id : x.Description;
                var sse = new NameTimeStringElement
                {
                    Time = x.UpdatedAt.ToDaysAgo(),
                    String = str,
                    Lines = 4,
                    Image = Theme.CurrentTheme.AnonymousUserImage,
                    Name = (x.Owner == null) ? "Anonymous" : x.Owner.Login,
                    ImageUri = (x.Owner == null) ? null : new Uri(x.Owner.AvatarUrl)
                };

                sse.Tapped += () => ViewModel.GoToGistCommand.Execute(x);
                return sse;
            });
        }
    }
}


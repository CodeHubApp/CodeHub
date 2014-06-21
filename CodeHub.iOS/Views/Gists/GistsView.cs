using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Gists;
using MonoTouch.Dialog;
using ReactiveUI;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class GistsView<TViewModel> : ViewModelCollectionView<TViewModel> where TViewModel : GistsViewModel
    {
        public override void ViewDidLoad()
        {
            NoItemsText = "No Gists";

            base.ViewDidLoad();

            Bind(ViewModel.WhenAnyValue(x => x.Gists), x =>
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


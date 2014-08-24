using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;

namespace CodeHub.iOS.Views.Gists
{
    public class PublicGistsView : BaseGistsView<PublicGistsViewModel>
    {
        public PublicGistsView()
        {
            Title = "Public Gists";
        }
    }
}
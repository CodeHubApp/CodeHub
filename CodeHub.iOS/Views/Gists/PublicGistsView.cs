using CodeHub.Core.ViewModels.Gists;

namespace CodeHub.iOS.Views.Gists
{
    public class PublicGistsView : GistsView<PublicGistsViewModel>
    {
        public PublicGistsView()
            : base(false)
        {
            Title = "Public Gists";
        }
    }
}
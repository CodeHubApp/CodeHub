using CodeHub.Core.ViewModels.Gists;

namespace CodeHub.iOS.Views.Gists
{
    public class PublicGistsView : GistsView<PublicGistsViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Public Gists";
            base.ViewDidLoad();
        }
    }
}
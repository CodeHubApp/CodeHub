using CodeHub.Core.ViewModels.Gists;

namespace CodeHub.iOS.Views.Gists
{
    public class StarredGistsView : GistsView<StarredGistsViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Starred Gists";
            base.ViewDidLoad();
        }
    }
}
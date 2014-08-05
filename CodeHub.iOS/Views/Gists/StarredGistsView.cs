using CodeHub.Core.ViewModels.Gists;

namespace CodeHub.iOS.Views.Gists
{
    public class StarredGistsView : GistsView<StarredGistsViewModel>
    {
        public StarredGistsView()
            : base(true)
        {
            Title = "Starred Gists";
        }
    }
}
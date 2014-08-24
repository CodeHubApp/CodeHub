using CodeHub.Core.ViewModels.Gists;

namespace CodeHub.iOS.Views.Gists
{
    public class StarredGistsView : BaseGistsView<StarredGistsViewModel>
    {
        public StarredGistsView()
        {
            Title = "Starred Gists";
        }
    }
}
namespace CodeHub.iOS.Views.Gists
{
    public class StarredGistsView : GistsView
    {
        public override void ViewDidLoad()
        {
            Title = "Starred Gists".t();
            base.ViewDidLoad();
        }
    }
}
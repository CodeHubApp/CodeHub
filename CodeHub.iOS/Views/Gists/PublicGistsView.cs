namespace CodeHub.iOS.Views.Gists
{
    public class PublicGistsView : GistsView
    {
        public override void ViewDidLoad()
        {
            Title = "Public Gists".t();
            base.ViewDidLoad();
        }
    }
}
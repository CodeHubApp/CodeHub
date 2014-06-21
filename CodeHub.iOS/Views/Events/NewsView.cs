using CodeHub.Core.ViewModels.Events;

namespace CodeHub.iOS.Views.Events
{
    public class NewsView : BaseEventsView<NewsViewModel>
    {
		public override void ViewDidLoad()
		{
			Title = "News";
			base.ViewDidLoad();
		}
    }
}


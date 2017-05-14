using CodeHub.Core.ViewModels.Events;

namespace CodeHub.iOS.ViewControllers.Events
{
    public class NewsViewController : BaseEventsViewController
    {
        public NewsViewController()
        {
            Title = "News";
        }

        public static NewsViewController Create()
            => new NewsViewController { ViewModel = new NewsViewModel() };
    }
}


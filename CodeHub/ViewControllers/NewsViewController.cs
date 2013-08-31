using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
    public class NewsViewController : EventsViewController
    {
        public NewsViewController()
            : base(string.Empty)
        {
            Title = "News".t();
            Controller = new NewsController(this);
        }
    }
}


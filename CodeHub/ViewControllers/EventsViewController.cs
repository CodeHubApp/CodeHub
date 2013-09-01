using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
    public class EventsViewController : BaseEventsViewController
    {
        public EventsViewController(string username)
        {
            Title = "Events".t();
            Controller = new EventsController(this, username);
        }
    }
}
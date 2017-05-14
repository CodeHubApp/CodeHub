using CodeHub.Core.ViewModels.Events;

namespace CodeHub.iOS.ViewControllers.Events
{
    public class UserEventsViewController : BaseEventsViewController
    {
        public UserEventsViewController()
        {
        }

        public UserEventsViewController(string username)
        {
            var viewModel = new UserEventsViewModel();
            viewModel.Init(new UserEventsViewModel.NavObject { Username = username });
            ViewModel = viewModel;
        }
    }
}
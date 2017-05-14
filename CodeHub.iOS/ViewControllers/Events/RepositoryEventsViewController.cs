using CodeHub.Core.ViewModels.Events;

namespace CodeHub.iOS.ViewControllers.Events
{
    public class RepositoryEventsViewController : BaseEventsViewController
    {
        public RepositoryEventsViewController()
        {
        }

        public RepositoryEventsViewController(string username, string repository)
        {
            var viewModel = new RepositoryEventsViewModel();
            viewModel.Init(new RepositoryEventsViewModel.NavObject
            {
                Username = username,
                Repository = repository
            });
            ViewModel = viewModel;
        }
    }
}


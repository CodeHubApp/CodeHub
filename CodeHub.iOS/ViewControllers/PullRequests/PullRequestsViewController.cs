using Octokit;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestsViewController : SegmentViewController
    {
        private readonly PullRequestListViewController _openViewController;
        private readonly PullRequestListViewController _closedViewController;

        public PullRequestsViewController(
            string username,
            string repository,
            ItemState state = ItemState.Open)
            : base(new string[] { "Open", "Closed" }, state == ItemState.Open ? 0 : 1)
        {
            _openViewController = new PullRequestListViewController(username, repository, ItemState.Open);
            _closedViewController = new PullRequestListViewController(username, repository, ItemState.Closed);
        }

        protected override void SegmentValueChanged(int id)
        {
            if (id == 0)
            {
                AddTable(_openViewController);
                RemoveIfLoaded(_closedViewController);
            }
            else
            {
                AddTable(_closedViewController);
                RemoveIfLoaded(_openViewController);
            }
        }
    }
}

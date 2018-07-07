using System;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class BranchesAndTagsViewController : SegmentViewController
    {
        private readonly BranchesViewController _branchesViewController;
        private readonly TagsViewController _tagsViewController;

        public IObservable<Octokit.Branch> BranchSelected => _branchesViewController.BranchSelected;

        public IObservable<Octokit.RepositoryTag> TagSelected => _tagsViewController.TagSelected;

        public enum SelectedView
        {
            Branches = 0,
            Tags
        }

        public BranchesAndTagsViewController(
            string username,
            string repository,
            SelectedView selectedView = SelectedView.Branches)
            : base(new string[] { "Branches", "Tags" }, (int)selectedView)
        {
            _branchesViewController = BranchesViewController.FromGitHub(username, repository);
            _tagsViewController = TagsViewController.FromGitHub(username, repository);
        }

        protected override void SegmentValueChanged(int id)
        {
            if (id == 0)
            {
                Title = "Branches";
                AddTable(_branchesViewController);
                RemoveIfLoaded(_tagsViewController);
            }
            else
            {
                Title = "Tags";
                AddTable(_tagsViewController);
                RemoveIfLoaded(_branchesViewController);
            }
        }
    }
}

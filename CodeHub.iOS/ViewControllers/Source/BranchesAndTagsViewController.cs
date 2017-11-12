using System;
using UIKit;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class BranchesAndTagsViewController : BaseViewController
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Branches", "Tags" });
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
        {
            _branchesViewController = new BranchesViewController(username, repository);
            _tagsViewController = new TagsViewController(username, repository);

            OnActivation(d =>
            {
                d(_viewSegment
                  .GetChangedObservable()
                  .Subscribe(SegmentValueChanged));
            });

            Appearing
                .Take(1)
                .Select(_ => (int)selectedView)
                .Do(x => _viewSegment.SelectedSegment = x)
                .Do(SegmentValueChanged)
                .Subscribe();

            NavigationItem.TitleView = _viewSegment;
        }

        private void SegmentValueChanged(int id)
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

        private void AddTable(UIViewController viewController)
        {
            viewController.View.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            AddChildViewController(viewController);
            Add(viewController.View);
        }

        private static void RemoveIfLoaded(UIViewController viewController)
        {
            viewController.RemoveFromParentViewController();
            viewController.ViewIfLoaded?.RemoveFromSuperview();
        }
    }
}

using System;
using UIKit;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class SegmentViewController : BaseViewController
    {
        private readonly UISegmentedControl _viewSegment;

        protected SegmentViewController(string[] segments, int selectedSegment = 0)
        {
            _viewSegment = new UISegmentedControl(segments);

            OnActivation(d =>
            {
                d(_viewSegment
                  .GetChangedObservable()
                  .Subscribe(SegmentValueChanged));
            });

            Appearing
                .Take(1)
                .Select(_ => selectedSegment)
                .Do(x => _viewSegment.SelectedSegment = x)
                .Do(SegmentValueChanged)
                .Do(x => Title = segments[x])
                .Subscribe();

            NavigationItem.TitleView = _viewSegment;
        }

        protected abstract void SegmentValueChanged(int id);

        protected void AddTable(UIViewController viewController)
        {
            viewController.View.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            AddChildViewController(viewController);
            Add(viewController.View);
        }

        protected static void RemoveIfLoaded(UIViewController viewController)
        {
            viewController.RemoveFromParentViewController();
            viewController.ViewIfLoaded?.RemoveFromSuperview();
        }
    }
}

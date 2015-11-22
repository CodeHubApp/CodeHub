using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.Transitions
{
    public class SlideDownTransition : UIViewControllerTransitioningDelegate
    {
        private TransitionAnimator _animator;

        public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForPresentedController(UIViewController presented, UIViewController presenting, UIViewController source)
        {
            _animator = new TransitionAnimator();
            _animator.Presenting = true;
            return _animator;
        }

        public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForDismissedController(UIViewController dismissed)
        {
            _animator = new TransitionAnimator();
            return _animator;
        }

        private class TransitionAnimator : UIViewControllerAnimatedTransitioning
        {
            public bool Presenting;

            public override double TransitionDuration(IUIViewControllerContextTransitioning transitionContext)
            {
                return 0.25f;
            }

            public override void AnimateTransition(IUIViewControllerContextTransitioning transitionContext)
            {
                var containerView = transitionContext.ContainerView;
                var toVC = transitionContext.GetViewControllerForKey (UITransitionContext.ToViewControllerKey);
                var fromVC = transitionContext.GetViewControllerForKey (UITransitionContext.FromViewControllerKey);

                if (Presenting)
                {
                    fromVC.View.UserInteractionEnabled = false;

                    containerView.AddSubview(fromVC.View);
                    containerView.AddSubview(toVC.View);

                    var endFrame = toVC.View.Frame;
                    var frame = toVC.View.Frame;
                    frame.Y = -frame.Height;
                    toVC.View.Frame = frame;

                    UIView.Animate(TransitionDuration(transitionContext), 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                    {
                        toVC.View.Frame = endFrame;
                    }, 
                        () =>
                    {
                        transitionContext.CompleteTransition(true);
                    });
                }
                else
                {
                    toVC.View.UserInteractionEnabled = true;

                    containerView.Add(toVC.View);
                    containerView.Add(fromVC.View);

                    var endFrame = fromVC.View.Frame;
                    endFrame.Y = -endFrame.Height;

                    UIView.Animate(TransitionDuration(transitionContext), 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                    {
                        fromVC.View.Frame = endFrame;
                    }, () =>
                    {
                        transitionContext.CompleteTransition(true);
                    });
                }
            }

        }
    }
}
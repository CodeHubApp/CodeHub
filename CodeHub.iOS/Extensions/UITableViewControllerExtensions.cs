// Analysis disable once CheckNamespace
namespace MonoTouch.UIKit
{
    public static class UITableViewControllerExtensions
    {
        public static UIView CreateTopBackground(this UITableViewController viewController, UIColor color)
        {
            var frame = viewController.TableView.Bounds;
            frame.Y = -frame.Size.Height;
            var view = new UIView(frame);
            view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            view.BackgroundColor = color;
            view.Layer.ZPosition = -1f;
            viewController.TableView.InsertSubview(view, 0);
            return view;
        }
    }
}
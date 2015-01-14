using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace CodeHub.iOS.DialogElements
{
    public interface IElementSizing 
    {
        float GetHeight (UITableView tableView, NSIndexPath indexPath);
    }
}


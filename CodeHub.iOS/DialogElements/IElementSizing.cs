using UIKit;
using Foundation;
using System;

namespace CodeHub.iOS.DialogElements
{
    public interface IElementSizing 
    {
        nfloat GetHeight (UITableView tableView, NSIndexPath indexPath);
    }
}


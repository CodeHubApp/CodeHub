using MonoTouch.Dialog;
using UIKit;

namespace CodeFramework.Elements
{
    public class CommentElement : NameTimeStringElement
    {
        protected override void OnCreateCell(UITableViewCell cell)
        {
            //Don't call the base since it will assign a background.
            return;
        }
    }
}


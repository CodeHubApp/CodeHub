using MonoTouch.Dialog;
using UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public class BaseDialogViewController : DialogViewController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDialogViewController"/> class.
        /// </summary>
        /// <param name="push">If set to <c>true</c> push.</param>
        public BaseDialogViewController(bool push)
            : base(new RootElement(""), push)
        {
			EdgesForExtendedLayout = UIRectEdge.None;
            Autorotate = true;
			SearchPlaceholder = "Search";
			//AutoHideSearch = true;
            Style = UITableViewStyle.Grouped;
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "" };
        }

		bool _appeared;
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if (!_appeared) {
				_appeared = true;
			}
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Title != null && Root != null)
                Root.Caption = Title;
        }
    }
}


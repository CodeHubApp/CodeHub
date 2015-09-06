using MonoTouch.Dialog;
using UIKit;
using System;

namespace CodeFramework.ViewControllers
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
				Flurry.Analytics.FlurryAgent.LogPageView ();
				Flurry.Analytics.FlurryAgent.LogEvent ("view:" + this.GetType ().Name);
				_appeared = true;
			}
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Title != null && Root != null)
                Root.Caption = Title;
        }

		public override DialogViewController.Source CreateSizingSource(bool unevenRows)
		{
			if (unevenRows)
				return (DialogViewController.Source)new SizingSource(this);
			else
				return (DialogViewController.Source)new Source(this);
		}

		protected new class SizingSource : DialogViewController.SizingSource
		{
			public SizingSource (DialogViewController controller) : base (controller) {}

			public override nfloat GetHeightForFooter (UITableView tableView, nint sectionIdx)
			{
                var section = Root[(int)sectionIdx];
				if (Container.Style == UITableViewStyle.Grouped && section.FooterView == null && string.IsNullOrEmpty(section.Footer))
					return 3;
				return base.GetHeightForFooter(tableView, sectionIdx);
			}

		}

		protected new class Source : DialogViewController.Source
		{
			public Source (DialogViewController controller) : base (controller) {}

			public override nfloat GetHeightForFooter (UITableView tableView, nint sectionIdx)
			{
                var section = Root[(int)sectionIdx];
				if (Container.Style == UITableViewStyle.Grouped && section.FooterView == null && string.IsNullOrEmpty(section.Footer))
					return 3;
				return base.GetHeightForFooter(tableView, sectionIdx);
			}
		}
    }
}


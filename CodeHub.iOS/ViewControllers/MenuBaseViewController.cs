using System.Drawing;
using MonoTouch.UIKit;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class MenuBaseViewController<TViewModel> : ViewModelDialogViewController<TViewModel> where TViewModel : class, IBaseViewModel
	{
        readonly MenuProfileView _profileButton;

        protected MenuBaseViewController()
            : base(style: UITableViewStyle.Plain)
        {
            _profileButton = new MenuProfileView(new RectangleF(0, 0, 320f, 44f));
            _profileButton.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            _profileButton.TouchUpInside += ProfileButtonClicked;
            NavigationItem.TitleView = _profileButton;
        }

        public MenuProfileView ProfileButton
        {
            get { return _profileButton; }
        }

		/// <summary>
		/// Invoked when it comes time to set the root so the child classes can create their own menus
		/// </summary>
		protected abstract void CreateMenuRoot();

        protected virtual void ProfileButtonClicked (object sender, System.EventArgs e)
        {
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Add some nice looking colors and effects
            TableView.SeparatorColor = UIColor.FromRGB(14, 14, 14);
            TableView.TableFooterView = new UIView(new RectangleF(0, 0, View.Bounds.Width, 0));
            TableView.BackgroundColor = UIColor.FromRGB(34, 34, 34);

            //Prevent the scroll to top on this view
            this.TableView.ScrollsToTop = false;
        }
        
        public override void ViewWillAppear(bool animated)
        {
            CreateMenuRoot();
            base.ViewWillAppear(animated);
        }
    }
}


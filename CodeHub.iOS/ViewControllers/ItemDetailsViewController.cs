using UIKit;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class ItemDetailsViewController : DialogViewController
    {
        protected readonly SlideUpTitleView SlideUpTitle;
        protected readonly ImageAndTitleHeaderView HeaderView;
        private readonly UIView _backgroundHeaderView;

        public override string Title
        {
            get
            {
                return base.Title;
            }
            set
            {
                HeaderView.Text = value;
                SlideUpTitle.Text = value;
                base.Title = value;
                RefreshHeaderView();
            }
        }

        protected ItemDetailsViewController(UITableViewStyle style = UITableViewStyle.Grouped)
            : base(style)
        {
            HeaderView = new ImageAndTitleHeaderView();
            SlideUpTitle = new SlideUpTitleView(44f) { Offset = 100f };
            NavigationItem.TitleView = SlideUpTitle;
            _backgroundHeaderView = new UIView();
        }

        public override UIRefreshControl RefreshControl
        {
            get { return base.RefreshControl; }
            set
            {
                if (value != null)
                    value.TintColor = UIColor.White;
                base.RefreshControl = value;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.ShadowImage = new UIImage();
            HeaderView.BackgroundColor = NavigationController.NavigationBar.BarTintColor;
            HeaderView.TextColor = NavigationController.NavigationBar.TintColor;
            HeaderView.SubTextColor = NavigationController.NavigationBar.TintColor.ColorWithAlpha(0.8f);
            _backgroundHeaderView.BackgroundColor = HeaderView.BackgroundColor;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (NavigationController != null)
                NavigationController.NavigationBar.ShadowImage = null;
        }

        protected void RefreshHeaderView()
        {
            TableView.TableHeaderView = HeaderView;
            TableView.ReloadData();
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            TableView.BeginUpdates();
            TableView.TableHeaderView = HeaderView;
            TableView.EndUpdates();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.TableHeaderView = HeaderView;
            TableView.SectionHeaderHeight = 0;

            var frame = TableView.Bounds;
            frame.Y = -frame.Size.Height;
            _backgroundHeaderView.Frame = frame;
            _backgroundHeaderView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _backgroundHeaderView.Layer.ZPosition = -1f;
            TableView.InsertSubview(_backgroundHeaderView, 0);
        }

        protected override void DidScroll(CoreGraphics.CGPoint p)
        {
            if (NavigationController == null)
                return;

            if (p.Y > 0)
                NavigationController.NavigationBar.ShadowImage = null;
            if (p.Y <= 0 && NavigationController.NavigationBar.ShadowImage == null)
                NavigationController.NavigationBar.ShadowImage = new UIImage();
            SlideUpTitle.Offset = 108 + 28 - p.Y;
        }
    }
}


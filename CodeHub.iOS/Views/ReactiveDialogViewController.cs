using System;
using Xamarin.Utilities.ViewControllers;
using MonoTouch.UIKit;
using System.Reactive.Linq;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.ViewComponents;
using Xamarin.Utilities.Delegates;
using Xamarin.Utilities.DialogElements;
using System.Drawing;

namespace CodeHub.iOS.Views
{
    public abstract class ReactiveDialogViewController
    {
        public static UIColor RefreshIndicatorColor = UIColor.Gray;
    }

    public abstract class ReactiveDialogViewController<TViewModel> : NewReactiveTableViewController<TViewModel> where TViewModel : class, IBaseViewModel
    {
        protected readonly SlideUpTitleView SlideUpTitle;
        protected readonly ImageAndTitleHeaderView HeaderView;
        private DialogTableViewSource _dialogSource;

        protected RootElement Root
        {
            get { return _dialogSource.Root; }
        }

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
                TableView.ReloadData();
            }
        }

        protected ReactiveDialogViewController()
            : base(UITableViewStyle.Grouped)
        {
            NavigationItem.TitleView = SlideUpTitle = new SlideUpTitleView(44f)
            {
                Offset = 100f,
            };



            HeaderView = new ImageAndTitleHeaderView();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.ShadowImage = new UIImage();

            HeaderView.BackgroundColor = NavigationController.NavigationBar.BackgroundColor;
            HeaderView.TextColor = NavigationController.NavigationBar.TintColor;
            HeaderView.SubTextColor = NavigationController.NavigationBar.TintColor.ColorWithAlpha(0.8f);
            (SlideUpTitle.Subviews[0] as UILabel).TextColor = HeaderView.TextColor;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.NavigationBar.ShadowImage = null;
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            TableView.ReloadData();
        }

        protected virtual DialogTableViewSource CreateTableViewSource()
        {
            return new DialogTableViewSource(TableView, true);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _dialogSource = CreateTableViewSource();

            _dialogSource.ScrolledObservable.Where(x => x.Y > 0)
                .Where(_ => NavigationController != null)
                .Subscribe(_ => NavigationController.NavigationBar.ShadowImage = null);
            _dialogSource.ScrolledObservable.Where(x => x.Y <= 0)
                .Where(_ => NavigationController != null)
                .Where(_ => NavigationController.NavigationBar.ShadowImage == null)
                .Subscribe(_ => NavigationController.NavigationBar.ShadowImage = new UIImage());
            _dialogSource.ScrolledObservable.Where(_ => SlideUpTitle != null).Subscribe(x => SlideUpTitle.Offset = 108 + 28f - x.Y);

            TableView.TableHeaderView = HeaderView;
            TableView.SectionHeaderHeight = 0;
            TableView.Source = _dialogSource;

            if (RefreshControl != null)
                RefreshControl.TintColor = ReactiveDialogViewController.RefreshIndicatorColor;

            var frame = TableView.Bounds;
            frame.Y = -frame.Size.Height;
            var view = new UIView(frame);
            view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            view.BackgroundColor = Theme.PrimaryNavigationBarColor;
            view.Layer.ZPosition = -1f;
            TableView.InsertSubview(view, 0);
        }
    }
}

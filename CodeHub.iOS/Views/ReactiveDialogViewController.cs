using System;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.Core.ViewModels;
using MonoTouch.UIKit;
using Xamarin.Utilities.Views;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views
{
    public abstract class ReactiveDialogViewController
    {
        public static UIColor RefreshIndicatorColor = UIColor.LightGray;
    }

    public abstract class ReactiveDialogViewController<TViewModel> : ViewModelDialogViewController<TViewModel> where TViewModel : class, IBaseViewModel
    {
        protected readonly SlideUpTitleView SlideUpTitle;
        protected readonly ImageAndTitleHeaderView HeaderView;

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
                ReloadData();
            }
        }

        protected ReactiveDialogViewController()
            : base(true)
        {
            NavigationItem.TitleView = SlideUpTitle = new SlideUpTitleView(44f)
            {
                Offset = 100f
            };

            HeaderView = new ImageAndTitleHeaderView();

            Scrolled.Where(x => x.Y > 0)
                .Where(_ => NavigationController != null)
                .Subscribe(_ => NavigationController.NavigationBar.ShadowImage = null);
            Scrolled.Where(x => x.Y <= 0)
                .Where(_ => NavigationController != null)
                .Where(_ => NavigationController.NavigationBar.ShadowImage == null)
                .Subscribe(_ => NavigationController.NavigationBar.ShadowImage = new UIImage());
            Scrolled.Where(_ => SlideUpTitle != null).Subscribe(x => SlideUpTitle.Offset = 108 + 28f - x.Y);
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
            ReloadData();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.SectionHeaderHeight = 0;

            if (RefreshControl != null)
                RefreshControl.TintColor = ViewModelPrettyDialogViewController.RefreshIndicatorColor;

            this.CreateTopBackground(NavigationController.NavigationBar.BackgroundColor);
        }
    }
}

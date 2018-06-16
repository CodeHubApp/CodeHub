using System;
using UIKit;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Views;
using CodeHub.iOS.Utilities;
using System.Linq;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class PrettyDialogViewController : ViewModelDrivenDialogViewController
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

        protected PrettyDialogViewController()
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

    public abstract class ViewModelDrivenDialogViewController : DialogViewController
    {
        public ReactiveObject ViewModel { get; protected set; }

        private bool _manualRefreshRequested;
  
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var loadableViewModel = ViewModel as LoadableViewModel;
            if (loadableViewModel != null)
            {
                RefreshControl = new UIRefreshControl();
                OnActivation(d =>
                {
                    d(loadableViewModel.LoadCommand.IsExecuting.Subscribe(x =>
                    {
                        if (x)
                        {
                            NetworkActivity.PushNetworkActive();
                            RefreshControl.BeginRefreshing();

                            if (!_manualRefreshRequested)
                            {
                                UIView.Animate(0.25, 0f, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseOut,
                                    () => TableView.ContentOffset = new CoreGraphics.CGPoint(0, -RefreshControl.Frame.Height), null);
                            }

                            foreach (var t in (ToolbarItems ?? Enumerable.Empty<UIBarButtonItem>()))
                                t.Enabled = false;
                        }
                        else
                        {
                            NetworkActivity.PopNetworkActive();

                            if (RefreshControl.Refreshing)
                            {
                                // Stupid bug...
                                BeginInvokeOnMainThread(() =>
                                {
                                    UIView.Animate(0.25, 0.0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseOut,
                                        () => TableView.ContentOffset = new CoreGraphics.CGPoint(0, 0), null);
                                    RefreshControl.EndRefreshing(); 
                                });
                            }

                            foreach (var t in (ToolbarItems ?? Enumerable.Empty<UIBarButtonItem>()))
                                t.Enabled = true;

                            _manualRefreshRequested = false;
                        }
                    }));
                });
            }
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        protected ViewModelDrivenDialogViewController(bool push = true, UITableViewStyle style = UITableViewStyle.Grouped)
            : base(style, push)
        {
        }

        private void HandleRefreshRequested(object sender, EventArgs e)
        {
            var loadableViewModel = ViewModel as LoadableViewModel;
            if (loadableViewModel != null)
            {
                _manualRefreshRequested = true;
                loadableViewModel.LoadCommand.ExecuteNow();
            }
        }

        bool _isLoaded = false;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (!_isLoaded)
            {
                var loadableViewModel = ViewModel as LoadableViewModel;
                if (loadableViewModel != null)
                    loadableViewModel.LoadCommand.ExecuteNow();
                _isLoaded = true;
            }

            if (RefreshControl != null)
                RefreshControl.ValueChanged += HandleRefreshRequested;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (RefreshControl != null)
                RefreshControl.ValueChanged -= HandleRefreshRequested;
        }
    }
}


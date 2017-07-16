using System;
using UIKit;
using System.Reactive.Linq;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class BaseDialogViewController : TableViewController
    {
        protected readonly SlideUpTitleView SlideUpTitle;
        protected readonly ImageAndTitleHeaderView HeaderView;
        private readonly UIView _backgroundHeaderView;
        private DialogTableViewSource _dialogSource;

        protected DialogTableViewSource DialogSource
        {
            get { return _dialogSource; }
        }

        protected RootElement Root{
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
                RefreshHeaderView();
            }
        }

        public override UIRefreshControl RefreshControl
        {
            get { return base.RefreshControl; }
            set
            {
                if (value != null) {
                    value.TintColor = UIColor.White.ColorWithAlpha(0.8f);
                }
                base.RefreshControl = value;
            }
        }

        protected BaseDialogViewController()
            : base(UITableViewStyle.Grouped)
        {
            SlideUpTitle = new SlideUpTitleView(44f) { Offset = 100f };
            NavigationItem.TitleView = SlideUpTitle;
            HeaderView = new ImageAndTitleHeaderView();
            _backgroundHeaderView = new UIView();

            Appearing
                .Where(x => ToolbarItems != null && NavigationController != null)
                .Subscribe(x => NavigationController.SetToolbarHidden(false, x));
            Disappearing
                .Where(x => ToolbarItems != null && NavigationController != null)
                .Subscribe(x => NavigationController.SetToolbarHidden(true, x));
            Disappearing
                .Where(_ => NavigationController != null)
                .Subscribe(_ => NavigationController.NavigationBar.ShadowImage = null);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            HeaderView.BackgroundColor = NavigationController.NavigationBar.BarTintColor;
            HeaderView.TextColor = NavigationController.NavigationBar.TintColor;
            HeaderView.SubTextColor = NavigationController.NavigationBar.TintColor.ColorWithAlpha(0.8f);
            (SlideUpTitle.Subviews[0] as UILabel).TextColor = HeaderView.TextColor;
            _backgroundHeaderView.BackgroundColor = HeaderView.BackgroundColor;
            TableView.TableHeaderView = HeaderView;
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            TableView.BeginUpdates();
            TableView.TableHeaderView = HeaderView;
            TableView.EndUpdates();
        }

        protected virtual DialogTableViewSource CreateTableViewSource()
        {
            return new DialogTableViewSource(TableView);
        }

        protected void RefreshHeaderView(string text = null, string subtext = null)
        {
            HeaderView.Text = text ?? HeaderView.Text;
            HeaderView.SubText = subtext ?? HeaderView.SubText;
            TableView.TableHeaderView = HeaderView;
            TableView.ReloadData();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _dialogSource = CreateTableViewSource();

            TableView.SectionHeaderHeight = 0;
            TableView.Source = _dialogSource;

            var frame = TableView.Bounds;
            frame.Y = -frame.Size.Height;
            _backgroundHeaderView.Frame = frame;
            _backgroundHeaderView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _backgroundHeaderView.Layer.ZPosition = -1f;
            TableView.InsertSubview(_backgroundHeaderView, 0);

            var scrollingObservable = _dialogSource.ScrolledObservable.Select(x => x.Y).StartWith(TableView.ContentOffset.Y);
            var shadowObs = scrollingObservable.Where(x => x > 0 && NavigationController != null);
            var shadowImgObs = scrollingObservable.Where(x => x <= 0).Where(_ => NavigationController != null)
                .Where(_ => NavigationController.NavigationBar.ShadowImage == null);
            var slideObs = scrollingObservable.Where(_ => SlideUpTitle != null);

            OnActivation(d => {
                d(shadowObs.Subscribe(_ => NavigationController.NavigationBar.ShadowImage = null));
                d(shadowImgObs.Subscribe(_ => NavigationController.NavigationBar.ShadowImage = new UIImage()));
                d(slideObs.Subscribe(x => SlideUpTitle.Offset = 108 + 28f - x));
            });
        }
    }
}

using System;
using CodeHub.Core.ViewModels.App;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using MonoTouch.UIKit;
using System.Reactive.Linq;
using Xamarin.Utilities.Views;

namespace CodeHub.iOS.Views.App
{
    public class FeedbackFeaturesView : ReactiveTableViewController<FeedbackFeaturesViewModel> 
    {
        private UIView _headerBackgroundView;
        private SlideUpTitleView _slideUpTitle = new SlideUpTitleView(44f);
        private readonly ImageAndTitleHeaderView _headerView = new ImageAndTitleHeaderView
        {
            Image = Images.Repo,
            SubText = "This app is a culmination of hard work and great suggestions! Please keep them coming!"
        };


        public override string Title
        {
            get
            {
                return base.Title;
            }
            set
            {
                if (_headerView != null) _headerView.Text = value;
                if (_slideUpTitle != null) _slideUpTitle.Text = value;
                base.Title = value;
            }
        }

        protected override void CreateRefreshControl()
        {
            base.CreateRefreshControl();
            RefreshControl.TintColor = NavigationController.NavigationBar.TintColor;
            TableView.SendSubviewToBack(_headerBackgroundView);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var slideupFrame = _slideUpTitle.Frame;
            slideupFrame.Height = NavigationController.NavigationBar.Bounds.Height;
            _slideUpTitle.Frame = slideupFrame;

            _headerView.BackgroundColor = NavigationController.NavigationBar.BackgroundColor;
            _headerView.TextColor = NavigationController.NavigationBar.TintColor;
            _headerView.SubTextColor = NavigationController.NavigationBar.TintColor.ColorWithAlpha(0.9f);

            var feedbackSource = new FeedbackTableViewSource(TableView, ViewModel.Items);
            feedbackSource.ScrolledObservable.Where(x => x.Y > 0)
                .Where(_ => NavigationController != null)
                .Subscribe(_ => NavigationController.NavigationBar.ShadowImage = null);
            feedbackSource.ScrolledObservable.Where(x => x.Y <= 0)
                .Where(_ => NavigationController != null)
                .Where(_ => NavigationController.NavigationBar.ShadowImage == null)
                .Subscribe(_ => NavigationController.NavigationBar.ShadowImage = new UIImage());
            feedbackSource.ScrolledObservable
                .StartWith(new System.Drawing.PointF(0, 0))
                .Subscribe(x => _slideUpTitle.Offset = 108 + 28f - x.Y);
            TableView.Source = feedbackSource;

            NavigationItem.TitleView = _slideUpTitle;
            TableView.TableHeaderView = _headerView;
            TableView.SectionHeaderHeight = 0;

            _headerBackgroundView = this.CreateTopBackground(NavigationController.NavigationBar.BackgroundColor);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.ShadowImage = new UIImage();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.NavigationBar.ShadowImage = null;
        }
    }
}


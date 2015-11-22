using System;
using UIKit;
using Foundation;
using ReactiveUI;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Services;

namespace CodeHub.iOS.ViewControllers
{
    public class WebBrowserViewController : BaseViewController<WebBrowserViewModel>, IModalViewController
    {
        private readonly UIBarButtonItem _closeButton = new UIBarButtonItem { Image = Images.Cancel };
        private readonly UIBarButtonItem _backButton = new UIBarButtonItem { Image = Images.BackChevron, Enabled = false };
        private readonly UIBarButtonItem _refreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh) { Enabled = false };
        private readonly UIBarButtonItem _forwardButton = new UIBarButtonItem { Image = Images.ForwardChevron, Enabled = false };
        private readonly UIWebView _web;

        public WebBrowserViewController()
            : this(true, true)
        {
        }

        public WebBrowserViewController(bool navigationToolbar, bool showPageAsTitle = false)
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "" };
            NavigationItem.LeftBarButtonItem = _closeButton;

            _web = new UIWebView {ScalesPageToFit = true};
            _web.ShouldStartLoad = (w, r, n) => true;

            _web.LoadStarted += (sender, e) => {
                NetworkActivityService.Instance.PushNetworkActive();
                _refreshButton.Enabled = false;
            };

            _web.LoadError += (sender, e) => {
                _refreshButton.Enabled = true;
            };

            _web.LoadFinished += (sender, e) => {
                NetworkActivityService.Instance.PopNetworkActive();
                _backButton.Enabled = _web.CanGoBack;
                _forwardButton.Enabled = _web.CanGoForward;
                _refreshButton.Enabled = true;

                if (showPageAsTitle)
                    Title = _web.EvaluateJavascript("document.title");
            };

            if (navigationToolbar)
            {
                ToolbarItems = new [] { 
                    _backButton,
                    new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 40f },
                    _forwardButton,
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    _refreshButton
                };
            }

            EdgesForExtendedLayout = UIRectEdge.None;

            this.WhenAnyValue(x => x.ViewModel.Uri)
                .SubscribeSafe(x => _web.LoadRequest(new NSUrlRequest(new NSUrl(x.AbsoluteUri))));

            OnActivation(d => {
                d(_backButton.GetClickedObservable().Subscribe(_ => _web.GoBack()));
                d(_forwardButton.GetClickedObservable().Subscribe(_ => _web.GoForward()));
                d(_refreshButton.GetClickedObservable().Subscribe(_ => _web.Reload()));
                d(_closeButton.GetClickedObservable().Subscribe(_ => DismissViewController(true, null)));
            });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            _web.Frame = View.Bounds;
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
            var bounds = View.Bounds;
            if (ToolbarItems != null)
                bounds.Height -= NavigationController.Toolbar.Frame.Height;

            Add(_web);
            _web.Frame = bounds;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _web.RemoveFromSuperview();
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            _web.Frame = View.Bounds;
        }
    }
}


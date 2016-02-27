using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Views;
using UIKit;
using WebKit;
using System;
using CodeHub.iOS.Services;
using System.Reactive.Linq;
using CodeHub.iOS.WebViews;

namespace CodeHub.iOS.Views.Repositories
{
    public class ReadmeView : WebView
    {
        private readonly UIBarButtonItem _actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action);

        public new ReadmeViewModel ViewModel
        {
            get { return (ReadmeViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public ReadmeView() : base(false)
        {
            Title = "Readme";
            NavigationItem.RightBarButtonItem = _actionButton;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.Bind(x => x.ContentText, true)
                .IsNotNull()
                .Select(x => new DescriptionModel(x, (int)UIFont.PreferredSubheadline.PointSize))
                .Select(x => new MarkdownView { Model = x }.GenerateString())
                .Subscribe(LoadContent);

            ViewModel.LoadCommand.Execute(false);
        }

        protected override bool ShouldStartLoad(WKWebView webView, WKNavigationAction navigationAction)
        {
            if (!navigationAction.Request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
            {
                ViewModel.GoToLinkCommand.Execute(navigationAction.Request.Url.AbsoluteString);
                return false;
            }

            return base.ShouldStartLoad(webView, navigationAction);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _actionButton.Clicked += ShareButtonPress;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _actionButton.Clicked -= ShareButtonPress;
        }

        private void ShareButtonPress(object o, EventArgs args)
        {
            var sheet = new UIActionSheet();
            var shareButton = sheet.AddButton("Share");
            var showButton = sheet.AddButton("Show in GitHub");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;

            sheet.Dismissed += (sender, e) =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    if (e.ButtonIndex == showButton)
                        ViewModel.GoToGitHubCommand.Execute(null);
                    else if (e.ButtonIndex == shareButton)
                        AlertDialogService.ShareUrl(ViewModel.HtmlUrl, o as UIBarButtonItem);
                });

                sheet.Dispose();
            };

            sheet.ShowFrom(_actionButton, true);
        }
    }
}


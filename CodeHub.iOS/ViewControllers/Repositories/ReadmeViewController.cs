using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core;
using CodeHub.Core.Services;
using CodeHub.iOS.Services;
using CodeHub.iOS.Views;
using CodeHub.WebViews;
using ReactiveUI;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class ReadmeViewController : BaseWebViewController
    {
        private static string LoadErrorMessage = "Unable to load readme.";
        private readonly UIBarButtonItem _actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action);
        private readonly IApplicationService _applicationService;
        private readonly IMarkdownService _markdownService;
        private readonly ObservableAsPropertyHelper<Octokit.Readme> _readme;
        private readonly string _owner;
        private readonly string _repository;

        private Octokit.Readme Readme => _readme.Value;

        public ReadmeViewController(
            string owner,
            string repository,
            Octokit.Readme readme = null,
            IApplicationService applicationService = null,
            IMarkdownService markdownService = null)
            : base(false, false)
        {
            _owner = owner;
            _repository = repository;
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _markdownService = markdownService ?? Locator.Current.GetService<IMarkdownService>();

            Title = "Readme";

            var loadCommand = ReactiveCommand.CreateFromTask(() =>
            {
                if (readme != null)
                    return Task.FromResult(readme);
                return _applicationService.GitHubClient.Repository.Content.GetReadme(owner, repository);
            });

            loadCommand
                .ThrownExceptions
                .Do(_ => SetErrorView())
                .Select(error => new UserError(LoadErrorMessage, error))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            loadCommand
                .ToProperty(this, x => x.Readme, out _readme);

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(loadCommand);

            this.WhenAnyValue(x => x.Readme)
                .Where(x => x != null)
                .SelectMany(ConvertToWebView)
                .Subscribe(LoadContent);

            this.WhenAnyValue(x => x.Readme)
                .Select(x => x != null)
                .Subscribe(x => _actionButton.Enabled = x);

            NavigationItem.RightBarButtonItem = _actionButton;

            this.WhenActivated(d =>
            {
                d(_actionButton.GetClickedObservable()
                  .Subscribe(ShareButtonPress));
            });
        }

        private void SetErrorView()
        {
            var emptyListView = new EmptyListView(Octicon.GitBranch.ToEmptyListImage(), LoadErrorMessage)
            {
                Alpha = 0,
                Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height)
            };

            View.Add(emptyListView);

            UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
                           () => emptyListView.Alpha = 1, null);
        }

        private async Task<string> ConvertToWebView(Octokit.Readme readme)
        {
            var content = await _markdownService.Convert(readme.Content);
            var model = new MarkdownModel(content, (int)UIFont.PreferredSubheadline.PointSize);
            return new MarkdownWebView { Model = model }.GenerateString();
        }

        protected override bool ShouldStartLoad(WebKit.WKWebView webView, WebKit.WKNavigationAction navigationAction)
        {
            if (!navigationAction.Request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
            {
                var viewController = new WebBrowserViewController(navigationAction.Request.Url.AbsoluteString);
                PresentViewController(viewController, true, null);
                return false;
            }

            return base.ShouldStartLoad(webView, navigationAction);
        }

        private void ShareButtonPress(UIBarButtonItem barButtonItem)
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
                    {
                        var viewController = new WebBrowserViewController(Readme.HtmlUrl);
                        PresentViewController(viewController, true, null);
                    }
                    else if (e.ButtonIndex == shareButton)
                    {
                        AlertDialogService.Share(
                            $"{_owner}/{_repository} Readme",
                            url: Readme.HtmlUrl,
                            barButtonItem: barButtonItem);
                    }
                });

                sheet.Dispose();
            };

            sheet.ShowFrom(barButtonItem, true);
        }
    }
}

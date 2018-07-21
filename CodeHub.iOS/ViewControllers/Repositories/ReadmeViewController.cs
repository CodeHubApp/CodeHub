using System;
using System.Reactive;
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
    public class ReadmeViewController : WebViewController
    {
        private static string LoadErrorMessage = "Unable to load readme.";
        private readonly UIBarButtonItem _actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action);
        private readonly IApplicationService _applicationService;
        private readonly IMarkdownService _markdownService;
        private readonly string _owner;
        private readonly string _repositoryName;

        private Octokit.Readme _readme;
        private Octokit.Readme Readme
        {
            get => _readme;
            set => this.RaiseAndSetIfChanged(ref _readme, value);
        }

        private Octokit.Repository _repository;
        private Octokit.Repository Repository
        {
            get => _repository;
            set => this.RaiseAndSetIfChanged(ref _repository, value);
        }

        public ReadmeViewController(
            string owner,
            string repositoryName,
            Octokit.Readme readme = null,
            Octokit.Repository repository = null,
            IApplicationService applicationService = null,
            IMarkdownService markdownService = null)
        {
            _owner = owner;
            _repositoryName = repositoryName;
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _markdownService = markdownService ?? Locator.Current.GetService<IMarkdownService>();

            Title = "Readme";

            var loadCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var readmeTask = readme != null
                    ? Task.FromResult(readme)
                    : _applicationService.GitHubClient.Repository.Content.GetReadme(owner, repositoryName);

                var repoTask = repository != null
                    ? Task.FromResult(repository)
                    : _applicationService.GitHubClient.Repository.Get(owner, repositoryName);

                await Task.WhenAll(readmeTask, repoTask);

                Readme = readmeTask.Result;
                Repository = repoTask.Result;
            });

            loadCommand
                .ThrownExceptions
                .SelectMany(SetError)
                .Subscribe();

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(loadCommand);

            this.WhenAnyValue(x => x.Readme, x => x.Repository)
                .Where(x => x.Item1 != null && x.Item2 != null)
                .SelectMany(x => ConvertToWebView(x.Item1, x.Item2))
                .Subscribe(LoadContent, err => SetError(err).Subscribe());

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

        private IObservable<Unit> SetError(Exception ex)
        {
            SetErrorView();
            var error = new UserError(LoadErrorMessage, ex);
            return Interactions.Errors.Handle(error);
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

        private async Task<string> ConvertToWebView(Octokit.Readme readme, Octokit.Repository repository)
        {
            var branch = System.Net.WebUtility.UrlDecode(repository.DefaultBranch);
            var baseUrl = $"{repository.HtmlUrl}/blob/{branch}/";
            var content = await _markdownService.Convert(readme.Content);
            var model = new MarkdownModel(content, (int)UIFont.PreferredSubheadline.PointSize, baseUrl: baseUrl);
            return new MarkdownWebView { Model = model }.GenerateString();
        }

        protected override bool ShouldStartLoad(WebKit.WKWebView webView, WebKit.WKNavigationAction navigationAction)
        {
            if (!navigationAction.Request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
            {
                this.PresentSafari(navigationAction.Request.Url.AbsoluteString);
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
                        this.PresentSafari(Readme.HtmlUrl);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core;
using CodeHub.Core.Services;
using CodeHub.iOS.Services;
using CodeHub.iOS.Utilities;
using CodeHub.WebViews;
using Foundation;
using Humanizer;
using Newtonsoft.Json;
using ReactiveUI;
using Splat;
using UIKit;
using WebKit;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestDiffViewController : BaseWebViewController
    {
        private readonly IApplicationService _applicationService;
        private readonly string _username;
        private readonly string _repository;
        private readonly int _pullRequestId;
        private readonly string _path;
        private readonly string _patch;
        private readonly string _commit;

        private readonly ReactiveList<Octokit.PullRequestReviewComment> _comments
             = new ReactiveList<Octokit.PullRequestReviewComment>();

        public PullRequestDiffViewController(
            string username,
            string repository,
            int pullRequestId,
            string path,
            string patch,
            string commit,
            IApplicationService applicationService = null)
            : base(false)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _username = username;
            _repository = repository;
            _pullRequestId = pullRequestId;
            _path = path;
            _patch = patch;
            _commit = commit;

            var loadComments = ReactiveCommand.CreateFromTask(
                _ => _applicationService.GitHubClient.PullRequest.ReviewComment.GetAll(_username, _repository, _pullRequestId));

            loadComments
                .ThrownExceptions
                .Select(error => new UserError("Unable to load comments: " + error.Message))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            loadComments
                .Subscribe(comments => _comments.Reset(comments));

            var loadAll = ReactiveCommand.CreateCombined(new[] { loadComments });

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(loadAll);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Observable
                .Return(Unit.Default)
                .Merge(_comments.Changed.Select(_ => Unit.Default))
                .Do(_ => Render())
                .Subscribe();
        }


        private void Render()
        {
            var comments = _comments
                .Where(x => string.Equals(x.Path, _path))
                .Select(comment => new DiffCommentModel
                {
                    Id = comment.Id,
                    Username = comment.User.Login,
                    AvatarUrl = comment.User.AvatarUrl,
                    LineTo = comment.Position,
                    LineFrom = comment.Position,
                    Body = comment.Body,
                    Date = comment.UpdatedAt.Humanize()
                });

            var diffModel = new DiffModel(
                _patch.Split('\n'),
                comments,
                (int)UIFont.PreferredSubheadline.PointSize);

            var diffView = new DiffWebView { Model = diffModel };
            LoadContent(diffView.GenerateString());
        }

        private class JavascriptCommentModel
        {
            public int PatchLine { get; set; }
            public int FileLine { get; set; }
        }

        protected override bool ShouldStartLoad(WKWebView webView, WKNavigationAction navigationAction)
        {
            var url = navigationAction.Request.Url;
            if (url.Scheme.Equals("app"))
            {
                var func = url.Host;
                if (func.Equals("comment"))
                {
                    var commentModel = JsonConvert.DeserializeObject<JavascriptCommentModel>(UrlDecode(url.Fragment));
                    PromptForComment(commentModel);
                }

                return false;
            }

            return base.ShouldStartLoad(webView, navigationAction);
        }

        private void PromptForComment(JavascriptCommentModel model)
        {
            var title = "Line " + model.FileLine;
            var sheet = new UIActionSheet(title);
            var addButton = sheet.AddButton("Add Comment");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.Dismissed += (sender, e) =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    if (e.ButtonIndex == addButton)
                        ShowCommentComposer(model.PatchLine);
                });

                sheet.Dispose();
            };

            sheet.ShowInView(this.View);
        }

        private void ShowCommentComposer(int line)
        {
            var composer = new MarkdownComposerViewController();
            composer.PresentAsModal(this, async () =>
            {
                UIApplication.SharedApplication.BeginIgnoringInteractionEvents();

                try
                {
                    var commentOptions = new Octokit.PullRequestReviewCommentCreate(composer.Text, _commit, _path, line);

                    await composer.DoWorkAsync("Commenting...", async () =>
                    {
                        var comment = await _applicationService.GitHubClient.PullRequest.ReviewComment.Create(
                            _username, _repository, _pullRequestId, commentOptions);
                        _comments.Add(comment);
                    });

                    this.DismissViewController(true, null);
                }
                catch (Exception e)
                {
                    AlertDialogService.ShowAlert("Unable to Comment", e.Message);
                }

                UIApplication.SharedApplication.EndIgnoringInteractionEvents();

            });
        }
    }
}


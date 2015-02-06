using System;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using ReactiveUI;
using Humanizer;
using CodeHub.WebViews;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views.Issues;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestView : BaseIssueView<PullRequestViewModel>
    {
        private readonly SplitViewElement _split1, _split2;
        private readonly HtmlElement _descriptionElement;
        private readonly HtmlElement _commentsElement;
        private readonly StyledStringElement _addCommentElement;
        private readonly StyledStringElement _commitsElement;
        private readonly StyledStringElement _filesElement;

        public PullRequestView()
        {
            _descriptionElement = new HtmlElement("body");
            _commentsElement = new HtmlElement("comments");

            _addCommentElement = new StyledStringElement("Add Comment") { Image = Images.Pencil };
            _addCommentElement.Tapped += AddCommentTapped;

            _split1 = new SplitViewElement
            {
                Button1 = new SplitViewElement.SplitButton(Images.Gear, string.Empty),
                Button2 = new SplitViewElement.SplitButton(Images.Gear, string.Empty)
            };

            _split2 = new SplitViewElement
            {
                Button1 = new SplitViewElement.SplitButton(Images.Gear, string.Empty),
                Button2 = new SplitViewElement.SplitButton(Images.Gear, string.Empty)
            };

            _commitsElement = new StyledStringElement("Commits", () => ViewModel.GoToCommitsCommand.ExecuteIfCan(), Images.Commit);
            _filesElement = new StyledStringElement("Files", () => ViewModel.GoToFilesCommand.ExecuteIfCan(), Images.Code);

            this.WhenViewModel(x => x.GoToUrlCommand).Subscribe(x =>
            {
                _commentsElement.UrlRequested = x.Execute;
                _descriptionElement.UrlRequested = x.Execute;
            });

            this.WhenViewModel(x => x.MarkdownDescription).Subscribe(x =>
            {
                var markdown = new DescriptionView { Model = x };
                var html = markdown.GenerateString();
                _descriptionElement.Value = html;
            });

            this.WhenViewModel(x => x.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }

//        private IEnumerable<CommentModel> CreateCommentList()
//        {
//            var items = ViewModel.Comments.Select(x => new CommentModel
//            { 
//                AvatarUrl = x.User.AvatarUrl, 
//                Login = x.User.Login, 
//                CreatedAt = x.CreatedAt,
//                Body = _markdownService.Convert(x.Body)
//            })
//                .Concat(ViewModel.Events.Select(x => new CommentModel
//            {
//                AvatarUrl = x.Actor.AvatarUrl, 
//                Login = x.Actor.Login, 
//                CreatedAt = x.CreatedAt,
//                Body = CreateEventBody(x.Event, x.CommitId)
//            })
//                    .Where(x => !string.IsNullOrEmpty(x.Body)));
//
//            return items.OrderBy(x => x.CreatedAt);
//        }
//
        public void RenderComments()
        {
//            var s = Cirrious.CrossCore.Mvx.Resolve<CodeFramework.Core.Services.IJsonSerializationService>();
//            var comments = CreateCommentList().Select(x => new {
//                avatarUrl = x.AvatarUrl,
//                login = x.Login,
//                created_at = x.CreatedAt.ToDaysAgo(),
//                body = x.Body
//            });
//            var data = s.Serialize(comments);
//
//            InvokeOnMainThread(() =>
//            {
//                _commentsElement.Value = !comments.Any() ? string.Empty : data;
//                if (_commentsElement.GetImmediateRootElement() == null)
//                    Render();
//            });
        }

        void AddCommentTapped()
        {
//            var composer = new MarkdownComposerViewController();
//            composer.NewComment(this, async text =>
//            {
//                var hud = this.CreateHud();
//                hud.Show("Posting Comment...");
//                if (await ViewModel.AddComment(text))
//                    composer.CloseComposer();
//
//                hud.Hide();
//                composer.EnableSendButton = true;
//            });
        }

        private void Render()
        {
            //Wait for the issue to load
            if (ViewModel.PullRequest == null)
                return;

            var sections = new List<Section>();
            sections.Add(new Section(HeaderView));

            var secDetails = new Section();
            if (!string.IsNullOrEmpty(ViewModel.PullRequest.Body))
                secDetails.Add(_descriptionElement);

            secDetails.Add(_split1);
            secDetails.Add(_split2);

            secDetails.Add(AssigneeElement);
            secDetails.Add(MilestoneElement);
            secDetails.Add(LabelsElement);
            sections.Add(secDetails);

            sections.Add(new Section { _commitsElement, _filesElement });

//            if (!(ViewModel.PullRequest.Merged != null && ViewModel.PullRequest.Merged.Value))
//            {
//                Action mergeAction = async () =>
//                {
////                    try
////                    {
////                        await this.DoWorkAsync("Merging...", ViewModel.Merge);
////                    }
////                    catch (Exception e)
////                    {
////                        MonoTouch.Utilities.ShowAlert("Unable to Merge", e.Message);
////                    }
//                };
//
////                StyledStringElement el;
////                if (ViewModel.PullRequest.Mergable == null)
////                    el = new StyledStringElement("Merge", mergeAction, Images.Fork);
////                else if (ViewModel.PullRequest.Mergable.Value)
////                    el = new StyledStringElement("Merge", mergeAction, Images.Fork);
////                else
////                    el = new StyledStringElement("Unable to merge!") { Image = Images.Fork };
////
//                sections.Add(new Section { el });
//            }
//
//            if (!string.IsNullOrEmpty(_commentsElement.Value))
//                root.Add(new Section { _commentsElement });

            sections.Add(new Section { _addCommentElement });

            Root.Reset(sections);
        }
    }
}


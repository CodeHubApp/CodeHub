using System;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.UIKit;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using ReactiveUI;
using Humanizer;
using CodeHub.WebViews;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestView : BaseDialogViewController<PullRequestViewModel>
    {
        private readonly SplitElement _split1, _split2;
        private readonly HtmlElement _descriptionElement;
        private readonly HtmlElement _commentsElement;
        private readonly StyledStringElement _milestoneElement;
        private readonly StyledStringElement _assigneeElement;
        private readonly StyledStringElement _labelsElement;
        private readonly StyledStringElement _addCommentElement;
        private readonly StyledStringElement _commitsElement;
        private readonly StyledStringElement _filesElement;

        public PullRequestView()
        {
            _descriptionElement = new HtmlElement("body");
            _commentsElement = new HtmlElement("comments");

            _milestoneElement = new StyledStringElement("Milestone", "No Milestone", UITableViewCellStyle.Value1) { Image = Images.Milestone };
            _milestoneElement.Tapped += () => ViewModel.GoToMilestoneCommand.ExecuteIfCan();

            _assigneeElement = new StyledStringElement("Assigned", "Unassigned", UITableViewCellStyle.Value1) { Image = Images.Person };
            _assigneeElement.Tapped += () => ViewModel.GoToAssigneeCommand.ExecuteIfCan();

            _labelsElement = new StyledStringElement("Labels", "None", UITableViewCellStyle.Value1) { Image = Images.Tag };
            _labelsElement.Tapped += () => ViewModel.GoToLabelsCommand.ExecuteIfCan();

            _addCommentElement = new StyledStringElement("Add Comment") { Image = Images.Pencil };
            _addCommentElement.Tapped += AddCommentTapped;

            _split1 = new SplitElement
            {
                Button1 = new SplitElement.SplitButton(Images.Gear, string.Empty),
                Button2 = new SplitElement.SplitButton(Images.Gear, string.Empty)
            };

            _split2 = new SplitElement
            {
                Button1 = new SplitElement.SplitButton(Images.Gear, string.Empty),
                Button2 = new SplitElement.SplitButton(Images.Gear, string.Empty)
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

            this.WhenViewModel(x => x.ShowMenuCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = x.ToBarButtonItem(UIBarButtonSystemItem.Action));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


            ViewModel.WhenAnyValue(x => x.PullRequest).IsNotNull().Subscribe(x =>
            {
//                var merged = (x.Merged != null && x.Merged.Value);

//                _split1.Value.Text1 = x.State;
//                _split1.Value.Text2 = merged ? "Merged" : "Not Merged";
//
//                _split2.Value.Text1 = x.User.Login;
//                _split2.Value.Text2 = x.CreatedAt.ToString("MM/dd/yy");

                HeaderView.ImageUri = x.User.AvatarUrl;
                HeaderView.Text = x.Title;
                HeaderView.SubText = "Updated " + x.UpdatedAt.UtcDateTime.Humanize();
                Render();
            });

            ViewModel.WhenAnyValue(x => x.Issue).IsNotNull().Subscribe(x =>
            {
                _assigneeElement.Value = x.Assignee != null ? x.Assignee.Login : "Unassigned";
                _milestoneElement.Value = x.Milestone != null ? x.Milestone.Title : "No Milestone";
                _labelsElement.Value = x.Labels.Count == 0 ? "None" : string.Join(", ", x.Labels.Select(i => i.Name));
                Render();
            });

            ViewModel.GoToLabelsCommand.CanExecuteChanged += (sender, e) =>
            {
                var before = _labelsElement.Accessory;
                _labelsElement.Accessory = ViewModel.GoToLabelsCommand.CanExecute(null) ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
                if (_labelsElement.Accessory != before && _labelsElement.GetRootElement() != null)
                    Root.Reload(_labelsElement, UITableViewRowAnimation.Fade);
            };
            ViewModel.GoToAssigneeCommand.CanExecuteChanged += (sender, e) =>
            {
                var before = _assigneeElement.Accessory;
                _assigneeElement.Accessory = ViewModel.GoToAssigneeCommand.CanExecute(null) ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
                if (_assigneeElement.Accessory != before && _assigneeElement.GetRootElement() != null)
                    Root.Reload(_assigneeElement, UITableViewRowAnimation.Fade);
            };
            ViewModel.GoToMilestoneCommand.CanExecuteChanged += (sender, e) =>
            {
                var before = _milestoneElement.Accessory;
                _milestoneElement.Accessory = ViewModel.GoToMilestoneCommand.CanExecute(null) ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
                if (_milestoneElement.Accessory != before && _milestoneElement.GetRootElement() != null)
                    Root.Reload(_milestoneElement, UITableViewRowAnimation.Fade);
            };
//
//            ViewModel.BindCollection(x => x.Comments, e => RenderComments());
//            ViewModel.BindCollection(x => x.Events, e => RenderComments());
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
        private static string CreateEventBody(string eventType, string commitId)
        {
            commitId = commitId ?? string.Empty;
            var smallCommit = commitId;
            if (string.IsNullOrEmpty(smallCommit))
                smallCommit = "Unknown";
            else if (smallCommit.Length > 7)
                smallCommit = commitId.Substring(0, 7);

            if (eventType == "closed")
                return "<p><span class=\"label label-danger\">Closed</span> this pull request.</p>";
            if (eventType == "reopened")
                return "<p><span class=\"label label-success\">Reopened</span> this pull request.</p>";
            if (eventType == "merged")
                return "<p><span class=\"label label-info\">Merged</span> commit " + smallCommit + "</p>";
            if (eventType == "referenced")
                return "<p><span class=\"label label-default\">Referenced</span> commit " + smallCommit + "</p>";
            return string.Empty;
        }

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

        public override UIView InputAccessoryView
        {
            get
            {
                var u = new UIView(new System.Drawing.RectangleF(0, 0, 320f, 27)) { BackgroundColor = UIColor.White };
                return u;
            }
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

            secDetails.Add(_assigneeElement);
            secDetails.Add(_milestoneElement);
            secDetails.Add(_labelsElement);
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


using System;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.UIKit;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestView : ViewModelPrettyDialogViewController<PullRequestViewModel>
    {
        private UIActionSheet _actionSheet;
        private SplitElement _split1, _split2;
        private WebElement _descriptionElement;
        private WebElement _commentsElement;
        private StyledStringElement _milestoneElement;
        private StyledStringElement _assigneeElement;
        private StyledStringElement _labelsElement;
        private StyledStringElement _addCommentElement;
        private readonly IMarkdownService _markdownService;

        public PullRequestView(IMarkdownService markdownService)
        {
            _markdownService = markdownService;
        }

        public override void ViewDidLoad()
        {
            Title = "Pull Request #" + ViewModel.PullRequestId;

            base.ViewDidLoad();

            var content = System.IO.File.ReadAllText("WebCell/body.html", System.Text.Encoding.UTF8);
            _descriptionElement = new WebElement("body");
            _descriptionElement.UrlRequested = ViewModel.GoToUrlCommand.Execute;

            var content2 = System.IO.File.ReadAllText("WebCell/comments.html", System.Text.Encoding.UTF8);
            _commentsElement = new WebElement("comments");
            _commentsElement.UrlRequested = ViewModel.GoToUrlCommand.Execute;

            _milestoneElement = new StyledStringElement("Milestone", "No Milestone", UITableViewCellStyle.Value1) { Image = Images.Milestone };
            _milestoneElement.Tapped += () => ViewModel.GoToMilestoneCommand.Execute(null);

            _assigneeElement = new StyledStringElement("Assigned", "Unassigned", UITableViewCellStyle.Value1) { Image = Images.Person };
            _assigneeElement.Tapped += () => ViewModel.GoToAssigneeCommand.Execute(null);

            _labelsElement = new StyledStringElement("Labels", "None", UITableViewCellStyle.Value1) { Image = Images.Tag };
            _labelsElement.Tapped += () => ViewModel.GoToLabelsCommand.Execute(null);

            _addCommentElement = new StyledStringElement("Add Comment") { Image = Images.Pencil };
            _addCommentElement.Tapped += AddCommentTapped;

            ViewModel.WhenAnyValue(x => x.MarkdownDescription).Subscribe(x => _descriptionElement.Value = x);
//
//            _split1 = new SplitElement(new SplitElement.Row { Image1 = Images.Cog, Image2 = Images.Merge });
//            _split2 = new SplitElement(new SplitElement.Row { Image1 = Images.Person, Image2 = Images.Create });

            ViewModel.WhenAnyValue(x => x.PullRequest).IsNotNull().Subscribe(x =>
            {
                var merged = (x.Merged != null && x.Merged.Value);

//                _split1.Value.Text1 = x.State;
//                _split1.Value.Text2 = merged ? "Merged" : "Not Merged";
//
//                _split2.Value.Text1 = x.User.Login;
//                _split2.Value.Text2 = x.CreatedAt.ToString("MM/dd/yy");

                HeaderView.ImageUri = x.User.AvatarUrl;
                HeaderView.Text = x.Title;
                HeaderView.SubText = "Updated " + x.UpdatedAt.ToDaysAgo();

                Render();
            });

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.PullRequest).Select(x => x != null));

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

        private IEnumerable<CommentModel> CreateCommentList()
        {
            var items = ViewModel.Comments.Select(x => new CommentModel
            { 
                AvatarUrl = x.User.AvatarUrl, 
                Login = x.User.Login, 
                CreatedAt = x.CreatedAt,
                Body = _markdownService.Convert(x.Body)
            })
                .Concat(ViewModel.Events.Select(x => new CommentModel
            {
                AvatarUrl = x.Actor.AvatarUrl, 
                Login = x.Actor.Login, 
                CreatedAt = x.CreatedAt,
                Body = CreateEventBody(x.Event, x.CommitId)
            })
                    .Where(x => !string.IsNullOrEmpty(x.Body)));

            return items.OrderBy(x => x.CreatedAt);
        }

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

        private void ShowExtraMenu()
        {
            if (ViewModel.PullRequest == null)
                return;

            var sheet = _actionSheet = new UIActionSheet(Title);
            var editButton = ViewModel.GoToEditCommand.CanExecute(null) ? sheet.AddButton("Edit") : -1;
            var openButton = sheet.AddButton(ViewModel.PullRequest.State == "open" ? "Close" : "Open");
            var commentButton = sheet.AddButton("Comment");
            var shareButton = ViewModel.ShareCommand.CanExecute(null) ? sheet.AddButton("Share") : -1;
            var showButton = sheet.AddButton("Show in GitHub");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Clicked += (s, e) =>
            {
                if (e.ButtonIndex == editButton)
                    ViewModel.GoToEditCommand.Execute(null);
                else if (e.ButtonIndex == openButton)
                    ViewModel.ToggleStateCommand.Execute(null);
                else if (e.ButtonIndex == shareButton)
                    ViewModel.ShareCommand.Execute(null);
                else if (e.ButtonIndex == showButton)
                    ViewModel.GoToUrlCommand.Execute(ViewModel.PullRequest.HtmlUrl);
                else if (e.ButtonIndex == commentButton)
                    AddCommentTapped();
                _actionSheet = null;
            };

            sheet.ShowInView(View);
        }

        private class CommentModel
        {
            public string AvatarUrl { get; set; }

            public string Login { get; set; }

            public DateTimeOffset CreatedAt { get; set; }

            public string Body { get; set; }
        }

        private void Render()
        {
            //Wait for the issue to load
            if (ViewModel.PullRequest == null)
                return;

            var sections = new List<Section>();
            sections.Add(new Section(HeaderView));

            var secDetails = new Section();
//            if (!string.IsNullOrEmpty(_descriptionElement.Value))
//                secDetails.Add(_descriptionElement);

            secDetails.Add(_split1);
            secDetails.Add(_split2);

            secDetails.Add(_assigneeElement);
            secDetails.Add(_milestoneElement);
            secDetails.Add(_labelsElement);
            sections.Add(secDetails);

            sections.Add(new Section
            {
                new StyledStringElement("Commits", () => ViewModel.GoToCommitsCommand.Execute(null), Images.Commit),
                new StyledStringElement("Files", () => ViewModel.GoToFilesCommand.Execute(null), Images.File),
            });

            if (!(ViewModel.PullRequest.Merged != null && ViewModel.PullRequest.Merged.Value))
            {
                Action mergeAction = async () =>
                {
//                    try
//                    {
//                        await this.DoWorkAsync("Merging...", ViewModel.Merge);
//                    }
//                    catch (Exception e)
//                    {
//                        MonoTouch.Utilities.ShowAlert("Unable to Merge", e.Message);
//                    }
                };

                StyledStringElement el;
                if (ViewModel.PullRequest.Mergable == null)
                    el = new StyledStringElement("Merge", mergeAction, Images.Fork);
                else if (ViewModel.PullRequest.Mergable.Value)
                    el = new StyledStringElement("Merge", mergeAction, Images.Fork);
                else
                    el = new StyledStringElement("Unable to merge!") { Image = Images.Fork };

                sections.Add(new Section { el });
            }
//
//            if (!string.IsNullOrEmpty(_commentsElement.Value))
//                root.Add(new Section { _commentsElement });

            sections.Add(new Section { _addCommentElement });

            Root.Reset(sections);
        }
    }
}


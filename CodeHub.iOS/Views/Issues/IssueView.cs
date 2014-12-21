using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using System.Linq;
using System.Collections.Generic;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using Humanizer;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueView : ReactiveDialogViewController<IssueViewModel>
    {
        protected WebElement _descriptionElement;
        protected WebElement _commentsElement;
        protected StyledStringElement _milestoneElement;
        protected StyledStringElement _assigneeElement;
        protected StyledStringElement _labelsElement;
        protected StyledStringElement _addCommentElement;
        private UIActionSheet _actionSheet;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var content = System.IO.File.ReadAllText("WebCell/body.html", System.Text.Encoding.UTF8);
            _descriptionElement = new WebElement("body");
            _descriptionElement.UrlRequested = ViewModel.GoToUrlCommand.ExecuteIfCan;
            //_descriptionElement.HeightChanged = x => Render();

            var content2 = System.IO.File.ReadAllText("WebCell/comments.html", System.Text.Encoding.UTF8);
            _commentsElement = new WebElement("comments");
            _commentsElement.UrlRequested = ViewModel.GoToUrlCommand.ExecuteIfCan;
            //_commentsElement.HeightChanged = x => Render();

            _milestoneElement = new StyledStringElement("Milestone", "No Milestone", UITableViewCellStyle.Value1) {Image = Images.Milestone, Accessory = UITableViewCellAccessory.DisclosureIndicator};
            _milestoneElement.Tapped += () => ViewModel.GoToMilestoneCommand.Execute(null);

            _assigneeElement = new StyledStringElement("Assigned", "Unassigned", UITableViewCellStyle.Value1) {Image = Images.Person, Accessory = UITableViewCellAccessory.DisclosureIndicator };
            _assigneeElement.Tapped += () => ViewModel.GoToAssigneeCommand.Execute(null);

            _labelsElement = new StyledStringElement("Labels", "None", UITableViewCellStyle.Value1) {Image = Images.Tag, Accessory = UITableViewCellAccessory.DisclosureIndicator};
            _labelsElement.Tapped += () => ViewModel.GoToLabelsCommand.Execute(null);

            _addCommentElement = new StyledStringElement("Add Comment") { Image = Images.Pencil };
            _addCommentElement.Tapped += AddCommentTapped;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.LoadCommand.IsExecuting.Select(x => !x));

            ViewModel.WhenAnyValue(x => x.Issue).Where(x => x != null).Subscribe(x =>
            {
                _assigneeElement.Value = x.Assignee != null ? x.Assignee.Login : "Unassigned";
                _milestoneElement.Value = x.Milestone != null ? x.Milestone.Title : "No Milestone";
                _labelsElement.Value = x.Labels.Count == 0 ? "None" : string.Join(", ", x.Labels.Select(i => i.Name));
                _descriptionElement.Value = ViewModel.MarkdownDescription;
                HeaderView.Text = x.Title;

                if (x.UpdatedAt.HasValue)
                    HeaderView.SubText = "Updated " + x.UpdatedAt.Value.UtcDateTime.Humanize();
                else
                    HeaderView.SubText = "Created " + x.CreatedAt.UtcDateTime.Humanize();

                HeaderView.ImageUri = x.User.AvatarUrl;
                Render();
            });
//
//            ViewModel.BindCollection(x => x.Comments, (e) => RenderComments());
//            ViewModel.BindCollection(x => x.Events, (e) => RenderComments());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            Title = "Issue #" + ViewModel.Id;
        }

        private IEnumerable<CommentModel> CreateCommentList()
        {
//            var items = ViewModel.Comments.Select(x => new CommentModel 
//            { 
//                AvatarUrl = x.User.AvatarUrl, 
//                Login = x.User.Login, 
//                CreatedAt = x.CreatedAt,
//                Body = ViewModel.ConvertToMarkdown(x.Body)
//            })
//                .Concat(ViewModel.Events.Select(x => new CommentModel
//            {
//                AvatarUrl = x.Actor.AvatarUrl, 
//                Login = x.Actor.Login, 
//                CreatedAt = x.CreatedAt,
//                Body = CreateEventBody(x.Event, x.CommitId)
//            })
//                .Where(x => !string.IsNullOrEmpty(x.Body)));
//
//            return items.OrderBy(x => x.CreatedAt);
            return null;
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
                return "<p><span class=\"label label-danger\">Closed</span> this issue.</p>";
            if (eventType == "reopened")
                return "<p><span class=\"label label-success\">Reopened</span> this issue.</p>";
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
//            InvokeOnMainThread(() => {
//                _commentsElement.Value = !comments.Any() ? string.Empty : data;
//                if (_commentsElement.GetImmediateRootElement() == null)
//                    Render();
//            });
        }

        protected virtual void Render()
        {
            //Wait for the issue to load
            if (ViewModel.Issue == null)
                return;

            var sections = new List<Section>();
            sections.Add(new Section(HeaderView));

            var secDetails = new Section();
//            if (!string.IsNullOrEmpty(_descriptionElement.Value))
//                secDetails.Add(_descriptionElement);

            secDetails.Add(_assigneeElement);
            secDetails.Add(_milestoneElement);
            secDetails.Add(_labelsElement);
            sections.Add(secDetails);

//            if (!string.IsNullOrEmpty(_commentsElement.Value))
//                root.Add(new Section { _commentsElement });

            sections.Add(new Section { _addCommentElement });
            Root.Reset(sections);
        }

        void AddCommentTapped()
        {
//            var composer = new MarkdownComposerViewController();
//            composer.NewComment(this, async (text) => {
//
//                var hud = this.CreateHud();
//                hud.Show("Posting Comment...");
//                if (await ViewModel.AddComment(text))
//                    composer.CloseComposer();
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
            if (ViewModel.Issue == null)
                return;

            _actionSheet = new UIActionSheet(Title);
            var editButton = _actionSheet.AddButton("Edit");
            var openButton = _actionSheet.AddButton(ViewModel.Issue.State == Octokit.ItemState.Open ? "Close" : "Open");
            var commentButton = _actionSheet.AddButton("Comment");
            var shareButton = _actionSheet.AddButton("Share");
            var showButton = _actionSheet.AddButton("Show in GitHub");
            var cancelButton = _actionSheet.AddButton("Cancel");
            _actionSheet.CancelButtonIndex = cancelButton;
            _actionSheet.DismissWithClickedButtonIndex(cancelButton, true);
            _actionSheet.Clicked += (s, e) => {
                if (e.ButtonIndex == editButton)
                    ViewModel.GoToEditCommand.Execute(null);
                else if (e.ButtonIndex == openButton)
                    ViewModel.ToggleStateCommand.Execute(null);
                else if (e.ButtonIndex == shareButton)
                    ViewModel.ShareCommand.Execute(ViewModel.Issue.HtmlUrl);
                else if (e.ButtonIndex == showButton)
                    ViewModel.GoToUrlCommand.Execute(ViewModel.Issue.HtmlUrl);
                else if (e.ButtonIndex == commentButton)
                    AddCommentTapped();
                _actionSheet = null;
            };

            _actionSheet.ShowInView(View);
        }

        private class CommentModel
        {
            public string AvatarUrl { get; set; }
            public string Login { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public string Body { get; set; }
        }
    }
}


using System;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;
using System.Linq;
using System.Collections.Generic;
using Humanizer;
using CodeHub.iOS.ViewControllers.Repositories;
using CodeHub.iOS.Services;
using System.Reactive.Linq;
using ReactiveUI;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueView : PrettyDialogViewController
    {
        protected WebElement _descriptionElement;
        protected WebElement _commentsElement;
        protected StringElement _milestoneElement;
        protected StringElement _assigneeElement;
        protected StringElement _labelsElement;
        protected StringElement _addCommentElement;
        private SplitButtonElement _split = new SplitButtonElement();
        private SplitButtonElement.Button _splitButton1;
        private SplitButtonElement.Button _splitButton2;

        public new IssueViewModel ViewModel
        {
            get { return (IssueViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _splitButton1 = _split.AddButton("Comments", "-");
            _splitButton2 = _split.AddButton("Participants", "-");

            Title = "Issue #" + ViewModel.Id;
            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = Title;

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => ViewModel.Bind(x => x.IsClosed, true).Where(x => x.HasValue).Select(x => x.Value))
                .Switch()
                .Subscribe(x => 
                {
                    HeaderView.SubImageView.TintColor = x ? UIColor.FromRGB(0xbd, 0x2c, 0) : UIColor.FromRGB(0x6c, 0xc6, 0x44);
                    HeaderView.SetSubImage((x ? Octicon.IssueClosed :Octicon.IssueOpened).ToImage());
                });

            var content = System.IO.File.ReadAllText("WebCell/body.html", System.Text.Encoding.UTF8);
            _descriptionElement = new WebElement(content, "body", false);

            var content2 = System.IO.File.ReadAllText("WebCell/comments.html", System.Text.Encoding.UTF8);
            _commentsElement = new WebElement(content2, "comments", true);

            _milestoneElement = new StringElement("Milestone", "No Milestone", UITableViewCellStyle.Value1) {Image = Octicon.Milestone.ToImage(), Accessory = UITableViewCellAccessory.DisclosureIndicator};
            _assigneeElement = new StringElement("Assigned", "Unassigned", UITableViewCellStyle.Value1) {Image = Octicon.Person.ToImage(), Accessory = UITableViewCellAccessory.DisclosureIndicator };
            _labelsElement = new StringElement("Labels", "None", UITableViewCellStyle.Value1) {Image = Octicon.Tag.ToImage(), Accessory = UITableViewCellAccessory.DisclosureIndicator};
            _addCommentElement = new StringElement("Add Comment") { Image = Octicon.Pencil.ToImage() };

            var actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action) { Enabled = false };
            NavigationItem.RightBarButtonItem = actionButton;

            ViewModel.Bind(x => x.IsLoading).Subscribe(x => 
            {
                if (!x)
                {
                    actionButton.Enabled = ViewModel.Issue != null;
                }
            });

            ViewModel.Bind(x => x.IsCollaborator).Subscribe(_ => Render());
            ViewModel.Bind(x => x.IsModifying).SubscribeStatus("Loading...");

            ViewModel.Bind(x => x.Issue).Subscribe(x =>
            {
                _assigneeElement.Value = x.Assignee != null ? x.Assignee.Login : "Unassigned";
                _milestoneElement.Value = x.Milestone != null ? x.Milestone.Title : "No Milestone";
                _labelsElement.Value = x.Labels.Count == 0 ? "None" : string.Join(", ", x.Labels.Select(i => i.Name));
                _descriptionElement.Value = ViewModel.MarkdownDescription;
 
                HeaderView.SubText = "Updated " + x.UpdatedAt.Humanize();
                HeaderView.SetImage(x.User?.AvatarUrl, Images.Avatar);
                RefreshHeaderView();

                Render();
            });

            ViewModel.BindCollection(x => x.Comments).Subscribe(_ => RenderComments());
            ViewModel.BindCollection(x => x.Events).Subscribe(_ => RenderComments());
            ViewModel.Bind(x => x.ShouldShowPro).Subscribe(x => {
                if (x) this.ShowPrivateView(); 
            });

            OnActivation(d =>
            {
                d(_milestoneElement.Clicked.BindCommand(ViewModel.GoToMilestoneCommand));
                d(_assigneeElement.Clicked.BindCommand(ViewModel.GoToAssigneeCommand));
                d(_labelsElement.Clicked.BindCommand(ViewModel.GoToLabelsCommand));
                d(_addCommentElement.Clicked.Subscribe(_ => AddCommentTapped()));
                d(_descriptionElement.UrlRequested.BindCommand(ViewModel.GoToUrlCommand));
                d(_commentsElement.UrlRequested.BindCommand(ViewModel.GoToUrlCommand));
                d(actionButton.GetClickedObservable().Subscribe(_ => ShowExtraMenu()));
            });
        }

        private IEnumerable<CommentModel> CreateCommentList()
        {
            var items = ViewModel.Comments.Select(x => new CommentModel 
            { 
                AvatarUrl = x.User.AvatarUrl, 
                Login = x.User.Login, 
                CreatedAt = x.CreatedAt.UtcDateTime,
                Body = ViewModel.ConvertToMarkdown(x.Body)
            })
                .Concat(ViewModel.Events.Select(x => new CommentModel
            {
                AvatarUrl = x.Actor.AvatarUrl, 
                Login = x.Actor.Login, 
                CreatedAt = x.CreatedAt.UtcDateTime,
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
            var s = MvvmCross.Platform.Mvx.Resolve<CodeHub.Core.Services.IJsonSerializationService>();
            var comments = CreateCommentList().Select(x => new {
                avatarUrl = x.AvatarUrl,
                login = x.Login,
                created_at = x.CreatedAt.Humanize(),
                body = x.Body
            });
            var data = s.Serialize(comments);

            InvokeOnMainThread(() => {
                _commentsElement.Value = !comments.Any() ? string.Empty : data;
                if (_commentsElement.GetRootElement() == null)
                    Render();
            });
        }

        protected virtual void Render()
        {
            //Wait for the issue to load
            if (ViewModel.Issue == null)
                return;

            var participants = ViewModel.Events.Select(x => x.Actor.Login).Distinct().Count();

            _splitButton1.Text = ViewModel.Issue.Comments.ToString();
            _splitButton2.Text = participants.ToString();

            ICollection<Section> sections = new LinkedList<Section>();
            sections.Add(new Section { _split });

            var secDetails = new Section();
            if (!string.IsNullOrEmpty(_descriptionElement.Value))
                secDetails.Add(_descriptionElement);

            foreach (var i in new [] { _assigneeElement, _milestoneElement, _labelsElement })
                i.Accessory = ViewModel.IsCollaborator ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;

            secDetails.Add(_assigneeElement);
            secDetails.Add(_milestoneElement);
            secDetails.Add(_labelsElement);
            sections.Add(secDetails);

            if (!string.IsNullOrEmpty(_commentsElement.Value))
                sections.Add(new Section { _commentsElement });

            sections.Add(new Section { _addCommentElement });
            Root.Reset(sections);
        }

        void AddCommentTapped()
        {
            var composer = new MarkdownComposerViewController();
            composer.NewComment(this, async (text) => {

                var hud = this.CreateHud();
                hud.Show("Posting Comment...");
                if (await ViewModel.AddComment(text))
                    composer.CloseComposer();
                hud.Hide();
                composer.EnableSendButton = true;
            });
        }

        public override UIView InputAccessoryView
        {
            get
            {
                var u = new UIView(new CoreGraphics.CGRect(0, 0, 320f, 27)) { BackgroundColor = UIColor.White };
                return u;
            }
        }

        private void ShowExtraMenu()
        {
            if (ViewModel.Issue == null)
                return;

            var sheet = new UIActionSheet();
            var editButton = ViewModel.IsCollaborator ? sheet.AddButton("Edit") : -1;
            var openButton = ViewModel.IsCollaborator ? sheet.AddButton(ViewModel.Issue.State == "open" ? "Close" : "Open") : -1;
            var commentButton = sheet.AddButton("Comment");
            var shareButton = sheet.AddButton("Share");
            var showButton = sheet.AddButton("Show in GitHub");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.Dismissed += (s, e) =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    if (e.ButtonIndex == editButton)
                        ViewModel.GoToEditCommand.Execute(null);
                    else if (e.ButtonIndex == openButton)
                        ViewModel.ToggleStateCommand.Execute(null);
                    else if (e.ButtonIndex == shareButton)
                        AlertDialogService.ShareUrl(ViewModel.Issue?.HtmlUrl, NavigationItem.RightBarButtonItem);
                    else if (e.ButtonIndex == showButton)
                        ViewModel.GoToUrlCommand.Execute(ViewModel.Issue.HtmlUrl);
                    else if (e.ButtonIndex == commentButton)
                        AddCommentTapped();
                });

                sheet.Dispose();
            };

            sheet.ShowInView(this.View);
        }

        private class CommentModel
        {
            public string AvatarUrl { get; set; }
            public string Login { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Body { get; set; }
        }
    }
}


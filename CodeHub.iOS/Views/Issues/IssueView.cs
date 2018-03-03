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
using CodeHub.WebViews;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using Splat;
using System.Reactive;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueView : PrettyDialogViewController
    {
        private readonly IMarkdownService _markdownService = Locator.Current.GetService<IMarkdownService>();
        private readonly HtmlElement _descriptionElement = new HtmlElement("description");
        private readonly HtmlElement _commentsElement = new HtmlElement("comments");
        private StringElement _milestoneElement;
        private StringElement _assigneeElement;
        private StringElement _labelsElement;
        private StringElement _addCommentElement;
        private SplitButtonElement _split = new SplitButtonElement();
        private SplitButtonElement.Button _splitButton1;
        private SplitButtonElement.Button _splitButton2;

        public new IssueViewModel ViewModel
        {
            get { return (IssueViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public IssueView(string owner, string repository, int id)
            : this()
        {
            ViewModel = new IssueViewModel();
            ViewModel.Init(new IssueViewModel.NavObject { Username = owner, Repository = repository, Id = id });
        }

        public IssueView()
        {
        }

        protected override void DidScroll(CoreGraphics.CGPoint p)
        {
            base.DidScroll(p);

            _descriptionElement.SetLayout();
            _commentsElement.SetLayout();
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

            _milestoneElement = new StringElement("Milestone", "No Milestone", UITableViewCellStyle.Value1) {Image = Octicon.Milestone.ToImage() };
            _assigneeElement = new StringElement("Assigned", "Unassigned", UITableViewCellStyle.Value1) {Image = Octicon.Person.ToImage() };
            _labelsElement = new StringElement("Labels", "None", UITableViewCellStyle.Value1) {Image = Octicon.Tag.ToImage() };
            _addCommentElement = new StringElement("Add Comment") { Image = Octicon.Pencil.ToImage() };

            var actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action) { Enabled = false };
            NavigationItem.RightBarButtonItem = actionButton;

            ViewModel.Bind(x => x.IsModifying).SubscribeStatus("Loading...");

            ViewModel.Bind(x => x.Issue).Subscribe(x =>
            {
                _assigneeElement.Value = x.Assignee != null ? x.Assignee.Login : "Unassigned";
                _milestoneElement.Value = x.Milestone != null ? x.Milestone.Title : "No Milestone";
                _labelsElement.Value = x.Labels.Count == 0 ? "None" : string.Join(", ", x.Labels.Select(i => i.Name));

                HeaderView.Text = x.Title;
                HeaderView.SubText = "Updated " + x.UpdatedAt.Humanize();
                HeaderView.SetImage(x.User?.AvatarUrl, Images.Avatar);
                RefreshHeaderView();

                Render();
            });

            ViewModel.Bind(x => x.MarkdownDescription).Subscribe(description =>
            {
                var model = new MarkdownModel(description, (int)UIFont.PreferredSubheadline.PointSize, true);
                var markdown = new MarkdownWebView { Model = model };
                var html = markdown.GenerateString();
                _descriptionElement.SetValue(string.IsNullOrEmpty(description) ? null : html);
                Render();
            });

            ViewModel
                .Bind(x => x.Comments)
                .Select(_ => Unit.Default)
                .Merge(ViewModel.Bind(x => x.Events).Select(_ => Unit.Default))
                .Subscribe(_ => RenderComments().ToBackground());

            ViewModel
                .Bind(x => x.Participants)
                .Subscribe(x => _splitButton2.Text = x.HasValue ? x.Value.ToString() : "-");

            ViewModel
                .Bind(x => x.ShouldShowPro)
                .Where(x => x)
                .Subscribe(x => this.ShowPrivateView());

            OnActivation(d =>
            {
                d(_milestoneElement.Clicked.BindCommand(ViewModel.GoToMilestoneCommand));
                d(_assigneeElement.Clicked.BindCommand(ViewModel.GoToAssigneeCommand));
                d(_labelsElement.Clicked.BindCommand(ViewModel.GoToLabelsCommand));
                d(_addCommentElement.Clicked.Subscribe(_ => AddCommentTapped()));
                d(_descriptionElement.UrlRequested.BindCommand(ViewModel.GoToUrlCommand));
                d(_commentsElement.UrlRequested.BindCommand(ViewModel.GoToUrlCommand));
                d(actionButton.GetClickedObservable().Subscribe(ShowExtraMenu));
                d(HeaderView.Clicked.BindCommand(ViewModel.GoToOwner));

                d(ViewModel.Bind(x => x.IsCollaborator, true).Subscribe(x => {
                    foreach (var i in new [] { _assigneeElement, _milestoneElement, _labelsElement })
                        i.Accessory = x ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
                }));

                d(ViewModel.Bind(x => x.IsLoading).Subscribe(x => actionButton.Enabled = !x));
            });
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

        public async Task RenderComments()
        {
            var comments = new List<Comment>();

            foreach (var x in ViewModel.Comments)
            {
                var body = await _markdownService.Convert(x.Body);
                comments.Add(new Comment(x.User.AvatarUrl, x.User.Login, body, x.CreatedAt));
            }

            var events = ViewModel
                .Events
                .Select(x => new Comment(x.Actor.AvatarUrl, x.Actor.Login, CreateEventBody(x.Event.StringValue, x.CommitId), x.CreatedAt));

            var items = comments
                .Concat(events)
                .Where(x => !string.IsNullOrEmpty(x.Body))
                .OrderBy(x => x.Date)
                .ToList();
            
            var commentModel = new CommentsModel(comments, (int)UIFont.PreferredSubheadline.PointSize);
            var razorView = new CommentsWebView { Model = commentModel };
            var html = razorView.GenerateString();

            InvokeOnMainThread(() => {
                _commentsElement.SetValue(!comments.Any() ? null : html);
                Render();
            });
        }

        protected virtual void Render()
        {
            //Wait for the issue to load
            if (ViewModel.Issue == null)
                return;

            _splitButton1.Text = ViewModel.Issue.Comments.ToString();

            ICollection<Section> sections = new LinkedList<Section>();
            sections.Add(new Section { _split });

            var secDetails = new Section();
            if (_descriptionElement.HasValue)
                secDetails.Add(_descriptionElement);

            secDetails.Add(_assigneeElement);
            secDetails.Add(_milestoneElement);
            secDetails.Add(_labelsElement);
            sections.Add(secDetails);

            var commentsSection = new Section();
            if (_commentsElement.HasValue)
                commentsSection.Add(_commentsElement);
            commentsSection.Add(_addCommentElement);
            sections.Add(commentsSection);

            Root.Reset(sections);
        }

        void AddCommentTapped()
        {
            var composer = new MarkdownComposerViewController();
            composer.PresentAsModal(this, async text =>
            {
                UIApplication.SharedApplication.BeginIgnoringInteractionEvents();

                var hud = this.CreateHud();
                hud.Show("Posting Comment...");
                if (await ViewModel.AddComment(text))
                    this.DismissViewController(true, null);
                hud.Hide();

                UIApplication.SharedApplication.EndIgnoringInteractionEvents();
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

        private void ShowExtraMenu(UIBarButtonItem item)
        {
            if (ViewModel.Issue == null)
                return;

            var issue = ViewModel.Issue;

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
                    {
                        AlertDialogService.Share(
                            Title,
                            issue.Body,
                            issue.HtmlUrl,
                            NavigationItem.RightBarButtonItem);
                    }
                    else if (e.ButtonIndex == showButton)
                        ViewModel.GoToUrlCommand.Execute(ViewModel.Issue.HtmlUrl);
                    else if (e.ButtonIndex == commentButton)
                        AddCommentTapped();
                });

                sheet.Dispose();
            };

            sheet.ShowFrom(item, true);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            Task.WhenAll(_descriptionElement.ForceResize(), _commentsElement.ForceResize())
                .ToBackground(() => Root.Reload(new [] { _commentsElement, _descriptionElement }));
        }
    }
}


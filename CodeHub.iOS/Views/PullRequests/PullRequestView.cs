using System;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.DialogElements;
using UIKit;
using System.Linq;
using System.Collections.Generic;
using Humanizer;
using CodeHub.iOS.Services;
using CodeHub.iOS.ViewControllers.Repositories;
using System.Reactive.Linq;
using ReactiveUI;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestView : PrettyDialogViewController
    {
        private WebElement _descriptionElement;
        private WebElement _commentsElement;
        private StringElement _milestoneElement;
        private StringElement _assigneeElement;
        private StringElement _labelsElement;
        private StringElement _addCommentElement;

        private readonly SplitViewElement _split1 = new SplitViewElement(Octicon.Gear.ToImage(), Octicon.GitMerge.ToImage());
        private readonly SplitViewElement _split2 = new SplitViewElement(Octicon.Person.ToImage(), Octicon.Calendar.ToImage());

        public new PullRequestViewModel ViewModel
        {
            get { return (PullRequestViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Pull Request #" + ViewModel.Id;
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

            _milestoneElement = new StringElement("Milestone", "No Milestone", UITableViewCellStyle.Value1) { Image = Octicon.Milestone.ToImage() };
            _assigneeElement = new StringElement("Assigned", "Unassigned", UITableViewCellStyle.Value1) { Image = Octicon.Person.ToImage() };
            _labelsElement = new StringElement("Labels", "None", UITableViewCellStyle.Value1) { Image = Octicon.Tag.ToImage() };
            _addCommentElement = new StringElement("Add Comment") { Image = Octicon.Pencil.ToImage() };

            ViewModel.Bind(x => x.PullRequest).Subscribe(x =>
            {
                var merged = (x.Merged != null && x.Merged.Value);
                _split1.Button1.Text = x.State;
                _split1.Button2.Text = merged ? "Merged" : "Not Merged";
                _split2.Button1.Text = x.User.Login;
                _split2.Button2.Text = x.CreatedAt.ToString("MM/dd/yy");
                _descriptionElement.Value = ViewModel.MarkdownDescription;
                HeaderView.SubText = "Updated " + x.UpdatedAt.Humanize();
                HeaderView.SetImage(x.User?.AvatarUrl, Images.Avatar);
                RefreshHeaderView();
                Render();
            });

            var actionButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action) { Enabled = false };

            ViewModel.Bind(x => x.IsLoading).Subscribe(x =>
            {
                if (!x)
                {
                    actionButton.Enabled = ViewModel.PullRequest != null;
                }
            });

            ViewModel.Bind(x => x.ShouldShowPro).Subscribe(x => {
                if (x) this.ShowPrivateView();
            });

            ViewModel.Bind(x => x.CanPush).Subscribe(_ => Render());
 

            ViewModel.GoToLabelsCommand.CanExecuteChanged += (sender, e) =>
            {
                _labelsElement.Accessory = ViewModel.GoToLabelsCommand.CanExecute(null) ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
            };
            ViewModel.GoToAssigneeCommand.CanExecuteChanged += (sender, e) =>
            {
                _assigneeElement.Accessory = ViewModel.GoToAssigneeCommand.CanExecute(null) ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
            };
            ViewModel.GoToMilestoneCommand.CanExecuteChanged += (sender, e) =>
            {
                _milestoneElement.Accessory = ViewModel.GoToMilestoneCommand.CanExecute(null) ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
            };

            ViewModel.BindCollection(x => x.Comments).Subscribe(_ => RenderComments());
            ViewModel.BindCollection(x => x.Events).Subscribe(_ => RenderComments());

            OnActivation(d =>
            {
                d(_milestoneElement.Clicked.BindCommand(ViewModel.GoToMilestoneCommand));
                d(_assigneeElement.Clicked.BindCommand(ViewModel.GoToAssigneeCommand));
                d(_labelsElement.Clicked.BindCommand(ViewModel.GoToLabelsCommand));
                d(_addCommentElement.Clicked.Subscribe(_ => AddCommentTapped()));
                d(_descriptionElement.UrlRequested.BindCommand(ViewModel.GoToUrlCommand));
                d(_commentsElement.UrlRequested.BindCommand(ViewModel.GoToUrlCommand));
                d(actionButton.GetClickedObservable().Subscribe(_ => ShowExtraMenu()));

                d(ViewModel.Bind(x => x.IsModifying).SubscribeStatus("Loading..."));

                d(ViewModel.Bind(x => x.Issue, true).Where(x => x != null).Subscribe(x =>
                {
                    _assigneeElement.Value = x.Assignee != null ? x.Assignee.Login : "Unassigned";
                    _milestoneElement.Value = x.Milestone != null ? x.Milestone.Title : "No Milestone";
                    _labelsElement.Value = x.Labels.Count == 0 ? "None" : string.Join(", ", x.Labels.Select(i => i.Name));
                    Render();
                }));
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
            var s = MvvmCross.Platform.Mvx.Resolve<CodeHub.Core.Services.IJsonSerializationService>();
            var comments = CreateCommentList().Select(x => new {
                avatarUrl = x.AvatarUrl,
                login = x.Login,
                created_at = x.CreatedAt.Humanize(),
                body = x.Body
            });
            var data = s.Serialize(comments);

            InvokeOnMainThread(() =>
            {
                _commentsElement.Value = !comments.Any() ? string.Empty : data;
                if (_commentsElement.GetRootElement() == null)
                    Render();
            });
        }

        void AddCommentTapped()
        {
            var composer = new MarkdownComposerViewController();
            composer.NewComment(this, async text =>
            {
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
            if (ViewModel.PullRequest == null)
                return;

            var sheet = new UIActionSheet();
            var editButton = ViewModel.GoToEditCommand.CanExecute(null) ? sheet.AddButton("Edit") : -1;
            var openButton = ViewModel.IsCollaborator ? sheet.AddButton(ViewModel.PullRequest.State == "open" ? "Close" : "Open") : -1;
            var commentButton = sheet.AddButton("Comment");
            var shareButton = !string.IsNullOrEmpty(ViewModel.PullRequest?.HtmlUrl) ? sheet.AddButton("Share") : -1;
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
                        AlertDialogService.ShareUrl(ViewModel.PullRequest?.HtmlUrl, NavigationItem.RightBarButtonItem);
                    else if (e.ButtonIndex == showButton)
                        ViewModel.GoToUrlCommand.Execute(ViewModel.PullRequest.HtmlUrl);
                    else if (e.ButtonIndex == commentButton)
                        AddCommentTapped();
                });

                sheet.Dispose();
            };

            sheet.ShowInView(View);
        }

        private class CommentModel
        {
            public string AvatarUrl { get; set; }
            public string Login { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Body { get; set; }
        }

        private void Render()
        {
            //Wait for the issue to load
            if (ViewModel.PullRequest == null)
                return;

            var additions = ViewModel.PullRequest?.Additions ?? 0;
            var deletions = ViewModel.PullRequest?.Deletions ?? 0;
            var changes = ViewModel.PullRequest?.ChangedFiles ?? 0;

            var split = new SplitButtonElement();
            split.AddButton("Additions", additions.ToString());
            split.AddButton("Deletions", deletions.ToString());
            split.AddButton("Changes", changes.ToString());

            ICollection<Section> sections = new LinkedList<Section>();
            sections.Add(new Section { split });

            var secDetails = new Section();
            if (!string.IsNullOrEmpty(_descriptionElement.Value))
                secDetails.Add(_descriptionElement);

            secDetails.Add(_split1);
            secDetails.Add(_split2);

            foreach (var i in new [] { _assigneeElement, _milestoneElement, _labelsElement })
                i.Accessory = ViewModel.IsCollaborator ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;

            secDetails.Add(_assigneeElement);
            secDetails.Add(_milestoneElement);
            secDetails.Add(_labelsElement);
            sections.Add(secDetails);

            var commits = new StringElement("Commits", Octicon.GitCommit.ToImage());
            commits.Clicked.Subscribe(_ => ViewModel.GoToCommitsCommand.Execute(null));

            var files = new StringElement("Files", Octicon.FileCode.ToImage());
            files.Clicked.Subscribe(_ => ViewModel.GoToFilesCommand.Execute(null));

            sections.Add(new Section { commits, files });

            if (ViewModel.CanPush && !(ViewModel.PullRequest.Merged != null && ViewModel.PullRequest.Merged.Value))
            {
                Action mergeAction = async () =>
                {
                    try
                    {
                        await this.DoWorkAsync("Merging...", ViewModel.Merge);
                    }
                    catch (Exception e)
                    {
                        AlertDialogService.ShowAlert("Unable to Merge", e.Message);
                    }
                };

                StringElement el;
                if (ViewModel.PullRequest.Mergable == null)
                {
                    el = new StringElement("Merge This Pull Request!", Octicon.GitMerge.ToImage());
                    el.Clicked.Subscribe(_ => mergeAction());
                }
                else if (ViewModel.PullRequest.Mergable.Value)
                {
                    el = new StringElement("Merge This Pull Request!", Octicon.GitMerge.ToImage());
                    el.Clicked.Subscribe(_ => mergeAction());
                }
                else
                    el = new StringElement("Unable to merge!") { Image = Octicon.GitMerge.ToImage() };
                el.Accessory = UITableViewCellAccessory.None;

                sections.Add(new Section { el });
            }

            if (!string.IsNullOrEmpty(_commentsElement.Value))
                sections.Add(new Section { _commentsElement });

            sections.Add(new Section { _addCommentElement });


            Root.Reset(sections);

        }
    }
}


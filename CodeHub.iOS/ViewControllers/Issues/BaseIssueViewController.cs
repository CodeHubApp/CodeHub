using System;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.DialogElements;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using System.Linq;
using CodeHub.iOS.ViewControllers;
using Humanizer;
using CodeHub.WebViews;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public abstract class BaseIssueViewController<TViewModel> : BaseDialogViewController<TViewModel> where TViewModel : BaseIssueViewModel
    {
        protected readonly SplitButtonElement SplitButton = new SplitButtonElement();
        protected readonly Section DetailsSection = new Section();
        protected readonly Section CommentsSection = new Section();
        protected readonly StringElement MilestoneElement;
        protected readonly StringElement AssigneeElement;
        protected readonly StringElement LabelsElement;
        protected readonly HtmlElement DescriptionElement;
        protected readonly HtmlElement CommentsElement;

        protected BaseIssueViewController()
        {
            CommentsElement = new HtmlElement("comments");
            this.WhenAnyValue(x => x.ViewModel.GoToUrlCommand)
                .Subscribe(x => CommentsElement.UrlRequested = x.ExecuteIfCan);

            DescriptionElement = new HtmlElement("description");
            this.WhenAnyValue(x => x.ViewModel.GoToUrlCommand)
                .Subscribe(x => DescriptionElement.UrlRequested = x.ExecuteIfCan);

            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyObservable(x => x.ViewModel.GoToAssigneesCommand)
                .Subscribe(_ => IssueAssigneeViewController.Show(this, ViewModel.CreateAssigneeViewModel()));

            this.WhenAnyObservable(x => x.ViewModel.GoToMilestonesCommand)
                .Subscribe(_ => IssueMilestonesViewController.Show(this, ViewModel.CreateMilestonesViewModel()));

            this.WhenAnyObservable(x => x.ViewModel.GoToLabelsCommand)
                .Subscribe(_ => IssueLabelsViewController.Show(this, ViewModel.CreateLabelsViewModel()));

            this.WhenAnyValue(x => x.ViewModel.GoToOwnerCommand)
                .Subscribe(x => HeaderView.ImageButtonAction = x != null ? new Action(() => ViewModel.GoToOwnerCommand.ExecuteIfCan()) : null);

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsClosed))
                .Switch()
                .Subscribe(x => 
                {
                    HeaderView.SubImageView.TintColor = x ? UIColor.FromRGB(0xbd, 0x2c, 0) : UIColor.FromRGB(0x6c, 0xc6, 0x44);
                    HeaderView.SetSubImage((x ? Octicon.IssueClosed :Octicon.IssueOpened).ToImage());
                });

            MilestoneElement = new StringElement("Milestone", string.Empty, UITableViewCellStyle.Value1) {Image = Octicon.Milestone.ToImage()};
            MilestoneElement.Tapped = () => ViewModel.GoToMilestonesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedMilestone)
                .Select(x => x == null ? "No Milestone" : x.Title)
                .Subscribe(x => MilestoneElement.Value = x);

            AssigneeElement = new StringElement("Assigned", string.Empty, UITableViewCellStyle.Value1) {Image = Octicon.Person.ToImage()};
            AssigneeElement.Tapped = () => ViewModel.GoToAssigneesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedUser)
                .Select(x => x == null ? "Unassigned" : x.Login)
                .Subscribe(x => AssigneeElement.Value = x);

            LabelsElement = new StringElement("Labels", string.Empty, UITableViewCellStyle.Value1) {Image = Octicon.Tag.ToImage()};
            LabelsElement.Tapped = () => ViewModel.GoToLabelsCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedLabels)
                .Select(x => (x == null || x.Count == 0) ? "None" : string.Join(",", x.Select(y => y.Name)))
                .Subscribe(x => LabelsElement.Value = x);

            this.WhenAnyValue(x => x.ViewModel.CanModify)
                .Select(x => x ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None)
                .Subscribe(x => MilestoneElement.Accessory = AssigneeElement.Accessory = LabelsElement.Accessory = x);

            DetailsSection.Add(MilestoneElement);
            DetailsSection.Add(AssigneeElement);
            DetailsSection.Add(LabelsElement);

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .SubscribeSafe(x => HeaderView.SetImage(x?.ToUri(128), Images.LoginUserUnknown));

            this.WhenAnyValue(x => x.ViewModel.Issue)
                .IsNotNull()
                .Subscribe(x => {
                    HeaderView.Text = x.Title;
                    HeaderView.SubText = x.UpdatedAt.HasValue ? 
                            ("Updated " + x.UpdatedAt.Value.UtcDateTime.Humanize()) :
                            ("Created " + x.CreatedAt.UtcDateTime.Humanize());
                    RefreshHeaderView();
                });

            this.WhenAnyValue(x => x.ViewModel.MarkdownDescription)
                .Subscribe(x =>
                {
                    if (string.IsNullOrEmpty(x))
                    {
                        DetailsSection.Remove(DescriptionElement);
                    }
                    else
                    {
                        var model = new DescriptionModel(x, (int)UIFont.PreferredSubheadline.PointSize);
                        var markdown = new MarkdownView { Model = model };
                        var html = markdown.GenerateString();
                        DescriptionElement.Value = html;

                        if (!DetailsSection.Contains(DescriptionElement))
                            DetailsSection.Insert(0, UITableViewRowAnimation.Fade, DescriptionElement);
                    }
                });

            CommentsSection.FooterView = new TableFooterButton("Add Comment", () => ViewModel.AddCommentCommand.ExecuteIfCan());

            this.WhenAnyValue(x => x.ViewModel.Events)
                .Select(x => x.Changed)
                .Switch()
                .Select(x => ViewModel.Events)
                .Subscribe(events =>
                {
                    var comments = events.Select(x => 
                    {
                        var body = string.Empty;
                        var comment = x as IssueCommentItemViewModel;
                        var @event = x as IssueEventItemViewModel;

                        if (comment != null)
                            body = comment.Comment;
                        else if (@event != null)
                            body = CreateEventBody(@event.EventInfo, @event.Commit);

                        return new Comment(x.AvatarUrl.ToUri(), x.Actor, body, x.CreatedAt.Humanize());
                    })
                    .Where(x => !string.IsNullOrEmpty(x.Body))
                    .ToList();

                    if (comments.Count > 0)
                    {
                        var commentModel = new CommentModel(comments, (int)UIFont.PreferredSubheadline.PointSize);
                        var razorView = new CommentsView { Model = commentModel };
                        var html = razorView.GenerateString();
                        CommentsElement.Value = html;

                        if (!CommentsSection.Contains(CommentsElement))
                            CommentsSection.Insert(0, UITableViewRowAnimation.Fade, CommentsElement);
                    }
                    else
                    {
                        CommentsSection.Remove(CommentsElement);
                    }
                });

            var commentsButton = SplitButton.AddButton("Comments", "-");
            var participantsButton = SplitButton.AddButton("Participants", "-");

            this.WhenAnyValue(x => x.ViewModel.CommentCount)
                .Subscribe(x => commentsButton.Text = x.ToString());

            this.WhenAnyValue(x => x.ViewModel.Participants)
                .Subscribe(x => participantsButton.Text = x.ToString());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Root.Reset(new Section { SplitButton }, DetailsSection, CommentsSection);
        }
            
        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            CommentsElement.CheckHeight();
            DescriptionElement.CheckHeight();
        }


        protected static string CreateEventBody(Octokit.EventInfo eventInfo, string commitId)
        {
            var eventType = eventInfo.Event;
            commitId = commitId ?? string.Empty;
            var smallCommit = commitId;
            if (string.IsNullOrEmpty(smallCommit))
                smallCommit = "Unknown";
            else if (smallCommit.Length > 7)
                smallCommit = commitId.Substring(0, 7);

//            if (eventType == Octokit.EventInfoState.Assigned)
//                return "<p><span class=\"label label-info\">Assigned</span> to this issue</p>";
            if (eventType == Octokit.EventInfoState.Closed)
                return "<p><span class=\"label label-danger\">Closed</span> this issue.</p>";
            if (eventType == Octokit.EventInfoState.Reopened)
                return "<p><span class=\"label label-success\">Reopened</span> this issue.</p>";
            if (eventType == Octokit.EventInfoState.Merged)
                return "<p><span class=\"label label-info\">Merged</span> commit " + smallCommit + "</p>";
            if (eventType == Octokit.EventInfoState.Referenced)
                return "<p><span class=\"label label-default\">Referenced</span> commit " + smallCommit + "</p>";
            if (eventType == Octokit.EventInfoState.Labeled)
            {
                var color = eventInfo.Label.Color;
                var text = eventInfo.Label.Name;
                return "<p>Added <span class=\"label label-info\" style=\"background-color: #" + color + ";\">" + text + "</span> to this issue.</p>";
            }
            return string.Empty;
        }

    }
}


using System;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.DialogElements;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using System.Linq;
using CodeHub.iOS.ViewControllers;
using Humanizer;
using CodeHub.iOS.ViewComponents;
using CodeHub.WebViews;

namespace CodeHub.iOS.Views.Issues
{
    public abstract class BaseIssueView<TViewModel> : BaseDialogViewController<TViewModel> where TViewModel : BaseIssueViewModel
    {
        protected readonly Section DetailsSection = new Section();
        protected readonly Section CommentsSection = new Section();
        protected readonly StyledStringElement MilestoneElement;
        protected readonly StyledStringElement AssigneeElement;
        protected readonly StyledStringElement LabelsElement;
        protected readonly HtmlElement DescriptionElement;
        protected readonly HtmlElement CommentsElement;

        protected BaseIssueView()
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

            this.WhenAnyValue(x => x.ViewModel.GoToAssigneesCommand)
                .Switch()
                .Subscribe(_ => ShowAssigneeSelector());

            this.WhenAnyValue(x => x.ViewModel.GoToMilestonesCommand)
                .Switch()
                .Subscribe(_ => ShowMilestonesSelector());

            this.WhenAnyValue(x => x.ViewModel.GoToLabelsCommand)
                .Switch()
                .Subscribe(_ => ShowLabelsSelector());

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
                    HeaderView.SetSubImage(x ? 
                        Images.IssueClosed.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) :
                        Images.IssueOpened.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate));
                });

            MilestoneElement = new StyledStringElement("Milestone", string.Empty, UITableViewCellStyle.Value1) {Image = Images.Milestone, Accessory = UITableViewCellAccessory.DisclosureIndicator};
            MilestoneElement.Tapped += () => ViewModel.GoToMilestonesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedMilestone)
                .Select(x => x == null ? "No Milestone" : x.Title)
                .Subscribe(x => MilestoneElement.Value = x);

            AssigneeElement = new StyledStringElement("Assigned", string.Empty, UITableViewCellStyle.Value1) {Image = Images.Person, Accessory = UITableViewCellAccessory.DisclosureIndicator };
            AssigneeElement.Tapped += () => ViewModel.GoToAssigneesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedUser)
                .Select(x => x == null ? "Unassigned" : x.Login)
                .Subscribe(x => AssigneeElement.Value = x);

            LabelsElement = new StyledStringElement("Labels", string.Empty, UITableViewCellStyle.Value1) {Image = Images.Tag, Accessory = UITableViewCellAccessory.DisclosureIndicator};
            LabelsElement.Tapped += () => ViewModel.GoToLabelsCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedLabels)
                .Select(x => (x == null || x.Count == 0) ? "None" : string.Join(",", x))
                .Subscribe(x => LabelsElement.Value = x);

            DetailsSection.Add(MilestoneElement);
            DetailsSection.Add(AssigneeElement);
            DetailsSection.Add(LabelsElement);

            this.WhenAnyValue(x => x.ViewModel.Issue)
                .IsNotNull()
                .Subscribe(x => 
                {
                    HeaderView.Text = x.Title;
                    HeaderView.ImageUri = x.User.AvatarUrl;

                    if (x.UpdatedAt.HasValue)
                        HeaderView.SubText = "Updated " + x.UpdatedAt.Value.UtcDateTime.Humanize();
                    else
                        HeaderView.SubText = "Created " + x.CreatedAt.UtcDateTime.Humanize();

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
                        var markdown = new DescriptionView { Model = x };
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

                        return new Comment(x.AvatarUrl, x.Actor, body, x.CreatedAt.LocalDateTime.Humanize());
                    })
                    .Where(x => !string.IsNullOrEmpty(x.Body))
                    .ToList();

                    if (comments.Count > 0)
                    {
                        var razorView = new CommentsView { Model = comments };
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
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Root.Reset(new Section(), DetailsSection, CommentsSection);
        }

        private void ShowAssigneeSelector()
        {
            var viewController = new IssueAssigneeView { Title = "Assignees" };
            viewController.ViewModel = ViewModel.Assignees;
            viewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, (s, e) => viewController.DismissViewController(true, null));
            PresentViewController(new ThemedNavigationController(viewController), true, null);
        }

        private void ShowLabelsSelector()
        {
            var viewController = new IssueLabelsView { Title = "Labels" };
            viewController.ViewModel = ViewModel.Labels;
            viewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, (s, e) => {
                ViewModel.Labels.SelectLabelsCommand.ExecuteIfCan();
                viewController.DismissViewController(true, null);
            });
            PresentViewController(new ThemedNavigationController(viewController), true, null);
        }

        private void ShowMilestonesSelector()
        {
            var viewController = new IssueMilestonesView { Title = "Milestones" };
            viewController.ViewModel = ViewModel.Milestones;
            viewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, (s, e) => viewController.DismissViewController(true, null));
            PresentViewController(new ThemedNavigationController(viewController), true, null);
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

            if (eventType == Octokit.EventInfoState.Assigned)
                return "<p><span class=\"label label-info\">Assigned</span> this issue</p>";
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


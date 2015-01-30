using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using System.Linq;
using System.Collections.Generic;
using ReactiveUI;
using Humanizer;
using CodeHub.WebViews;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueView : BaseDialogViewController<IssueViewModel>
    {
        private readonly StyledStringElement _milestoneElement;
        private readonly StyledStringElement _assigneeElement;
        private readonly StyledStringElement _labelsElement;

        public IssueView()
        {
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
                });

            _milestoneElement = new StyledStringElement("Milestone", string.Empty, UITableViewCellStyle.Value1) {Image = Images.Milestone, Accessory = UITableViewCellAccessory.DisclosureIndicator};
            _milestoneElement.Tapped += () => ViewModel.GoToMilestonesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedMilestone)
                .Select(x => x == null ? "No Milestone" : x.Title)
                .Subscribe(x => {
                    _milestoneElement.Value = x;
                    Root.Reload(_milestoneElement);
                });

            _assigneeElement = new StyledStringElement("Assigned", string.Empty, UITableViewCellStyle.Value1) {Image = Images.Person, Accessory = UITableViewCellAccessory.DisclosureIndicator };
            _assigneeElement.Tapped += () => ViewModel.GoToAssigneesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedUser)
                .Select(x => x == null ? "Unassigned" : x.Login)
                .Subscribe(x => {
                    _assigneeElement.Value = x;
                    Root.Reload(_assigneeElement);
                });

            _labelsElement = new StyledStringElement("Labels", string.Empty, UITableViewCellStyle.Value1) {Image = Images.Tag, Accessory = UITableViewCellAccessory.DisclosureIndicator};
            _labelsElement.Tapped += () => ViewModel.GoToLabelsCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedLabels)
                .Select(x => (x == null || x.Count == 0) ? "None" : string.Join(",", x))
                .Subscribe(x => {
                    _labelsElement.Value = x;
                    Root.Reload(_labelsElement);
                });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var secDetails = new Section();
            var commentsSection = new Section(null, new TableFooterButton("Add Comment", () => ViewModel.AddCommentCommand.ExecuteIfCan()));
            var commentsElement = new HtmlElement("comments");
            commentsElement.UrlRequested = ViewModel.GoToUrlCommand.ExecuteIfCan;

            var descriptionElement = new HtmlElement("description");
            descriptionElement.UrlRequested = ViewModel.GoToUrlCommand.ExecuteIfCan;

            secDetails.Add(_milestoneElement);
            secDetails.Add(_assigneeElement);
            secDetails.Add(_labelsElement);

            var addCommentElement = new StyledStringElement("Add Comment") { Image = Images.Pencil };

            this.WhenViewModel(x => x.MarkdownDescription).Subscribe(x =>
            {
                if (string.IsNullOrEmpty(x))
                {
                    secDetails.Remove(descriptionElement);
                }
                else
                {
                    var markdown = new DescriptionView { Model = x };
                    var html = markdown.GenerateString();
                    descriptionElement.Value = html;

                    if (!secDetails.Contains(descriptionElement))
                        secDetails.Insert(0, UITableViewRowAnimation.Fade, descriptionElement);
                }

            });

            ViewModel.Events.Changed.Subscribe(_ =>
            {
                var commentModels = ViewModel.Events
                    .Select(x => 
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

                if (commentModels.Count > 0)
                {
                    var razorView = new CommentsView { Model = commentModels };
                    var html = razorView.GenerateString();
                    commentsElement.Value = html;

                    if (!commentsSection.Contains(commentsElement))
                        commentsSection.Insert(0, UITableViewRowAnimation.Fade, commentsElement);
                }
                else
                {
                    commentsSection.Remove(commentsElement);
                }
            });

            Root.Reset(new Section(), secDetails, commentsSection);
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

        private static string CreateEventBody(Octokit.EventInfo eventInfo, string commitId)
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

        public override UIView InputAccessoryView
        {
            get
            {
                var u = new UIView(new CoreGraphics.CGRect(0, 0, 320f, 27)) { BackgroundColor = UIColor.White };
                return u;
            }
        }

    }
}


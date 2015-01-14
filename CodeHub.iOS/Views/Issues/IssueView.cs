using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using System.Linq;
using System.Collections.Generic;
using ReactiveUI;
using Humanizer;
using CodeHub.WebViews;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueView : BaseDialogViewController<IssueViewModel>
    {
        public IssueView()
        {
            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand).IsNotNull().Subscribe(x =>
                NavigationItem.RightBarButtonItem = x.ToBarButtonItem(UIBarButtonSystemItem.Action));
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

            var milestoneElement = new StyledStringElement("Milestone", "No Milestone", UITableViewCellStyle.Value1) {Image = Images.Milestone, Accessory = UITableViewCellAccessory.DisclosureIndicator};
            milestoneElement.Tapped += () => ViewModel.GoToMilestoneCommand.Execute(null);
            secDetails.Add(milestoneElement);

            var assigneeElement = new StyledStringElement("Assigned", "Unassigned", UITableViewCellStyle.Value1) {Image = Images.Person, Accessory = UITableViewCellAccessory.DisclosureIndicator };
            assigneeElement.Tapped += () => ViewModel.GoToAssigneeCommand.Execute(null);
            secDetails.Add(assigneeElement);

            var labelsElement = new StyledStringElement("Labels", "None", UITableViewCellStyle.Value1) {Image = Images.Tag, Accessory = UITableViewCellAccessory.DisclosureIndicator};
            labelsElement.Tapped += () => ViewModel.GoToLabelsCommand.Execute(null);
            secDetails.Add(labelsElement);

            var addCommentElement = new StyledStringElement("Add Comment") { Image = Images.Pencil };

            ViewModel.WhenAnyValue(x => x.Issue).Where(x => x != null).Subscribe(x =>
            {
                assigneeElement.Value = x.Assignee != null ? x.Assignee.Login : "Unassigned";
                milestoneElement.Value = x.Milestone != null ? x.Milestone.Title : "No Milestone";
                labelsElement.Value = x.Labels.Count == 0 ? "None" : string.Join(", ", x.Labels.Select(i => i.Name));
                HeaderView.Text = x.Title;
                HeaderView.ImageUri = x.User.AvatarUrl;

                if (x.UpdatedAt.HasValue)
                    HeaderView.SubText = "Updated " + x.UpdatedAt.Value.UtcDateTime.Humanize();
                else
                    HeaderView.SubText = "Created " + x.CreatedAt.UtcDateTime.Humanize();
            });

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
                var u = new UIView(new System.Drawing.RectangleF(0, 0, 320f, 27)) { BackgroundColor = UIColor.White };
                return u;
            }
        }
    }
}


using System;
using UIKit;
using System.Linq;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using System.Collections.Generic;
using CodeHub.WebViews;
using Humanizer;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class CommitViewController : BaseDialogViewController<CommitViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.Image = Images.LoginUserUnknown;

            var split = new SplitButtonElement();
            var headerSection = new Section { split };
            var descriptionElement = new MultilinedElement();
            var detailsSection = new Section();
            var footerButton = new TableFooterButton("Add Comment");
            var commentsSection = new Section(null, footerButton);

            var diffButton = new StringElement(Octicon.DiffAdded.ToImage());
            var removedButton = new StringElement(Octicon.DiffRemoved.ToImage());
            var modifiedButton = new StringElement(Octicon.DiffModified.ToImage());
            var allChangesButton = new StringElement("All Changes", Octicon.Diff.ToImage());
            detailsSection.Add(new [] { diffButton, removedButton, modifiedButton, allChangesButton });

            var additions = split.AddButton("Additions");
            var deletions = split.AddButton("Deletions");
            var parents = split.AddButton("Parents");
            var commentsElement = new HtmlElement("comments");

            ViewModel.Comments.Changed
                .Select(_ => new Unit())
                .StartWith(new Unit())
                .Subscribe(x =>
                {
                    var comments = ViewModel.Comments.Select(c => new Comment(c.Avatar.ToUri(), c.Actor, c.Body, c.UtcCreatedAt.Humanize())).ToList();
                    var commentModel = new CommentModel(comments, (int)UIFont.PreferredSubheadline.PointSize);
                    var razorView = new CommentsView { Model = commentModel };
                    var html = razorView.GenerateString();
                    commentsElement.Value = html;

                    if (commentsElement.GetRootElement() == null && ViewModel.Comments.Count > 0)
                        commentsSection.Add(commentsElement);
                    TableView.ReloadData();
                });

            this.WhenAnyValue(x => x.ViewModel.Commit)
                .IsNotNull()
                .Take(1)
                .Subscribe(_ => Root.Reset(headerSection, detailsSection, commentsSection));

            Appeared
                .Take(1)
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.CommitMessage, y => y.ViewModel.CommiterName))
                .Switch()
                .Where(x => x.Item1 != null && x.Item2 != null)
                .Take(1)
                .Subscribe(_ => detailsSection.Insert(0, UITableViewRowAnimation.Automatic, descriptionElement));

            OnActivation(d => {
                d(allChangesButton.BindCommand(ViewModel.GoToAllFiles));
                d(footerButton.Clicked.InvokeCommand(ViewModel.AddCommentCommand));

                d(this.WhenAnyValue(x => x.ViewModel.Avatar)
                    .Subscribe(x => HeaderView.SetImage(x?.ToUri(128), Images.LoginUserUnknown)));

                d(this.WhenAnyValue(x => x.ViewModel.CommitMessage)
                    .Subscribe(x => descriptionElement.Details = x ?? string.Empty));
                
                d(descriptionElement.BindCaption(this.WhenAnyValue(x => x.ViewModel.CommiterName)));

                d(diffButton.BindCommand(ViewModel.GoToAddedFiles));
                d(diffButton.BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffAdditions).Select(x => x > 0)));
                d(diffButton.BindCaption(this.WhenAnyValue(x => x.ViewModel.DiffAdditions).StartWith(0).Select(x => string.Format("{0} added", x))));

                d(removedButton.BindCommand(ViewModel.GoToRemovedFiles));
                d(removedButton.BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffDeletions).Select(x => x > 0)));
                d(removedButton.BindCaption(this.WhenAnyValue(x => x.ViewModel.DiffDeletions).StartWith(0).Select(x => string.Format("{0} removed", x))));

                d(modifiedButton.BindCommand(ViewModel.GoToModifiedFiles));
                d(modifiedButton.BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffModifications).Select(x => x > 0)));
                d(modifiedButton.BindCaption(this.WhenAnyValue(x => x.ViewModel.DiffModifications).StartWith(0).Select(x => string.Format("{0} modified", x))));

                d(allChangesButton.BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffAdditions, x => x.ViewModel.DiffDeletions, x => x.ViewModel.DiffModifications)
                    .Select(x => x.Item1 + x.Item2 + x.Item3 > 0)));

                d(commentsElement.UrlRequested.InvokeCommand(ViewModel.GoToUrlCommand));

                d(this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Action, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.CommitMessageSummary).Subscribe(x => RefreshHeaderView(x)));

                d(this.WhenAnyValue(x => x.ViewModel.Commit).IsNotNull()
                    .Select(x => "Commited " + x.Commit.Committer.Date.LocalDateTime.Humanize())
                    .Subscribe(x => RefreshHeaderView(subtext: x)));

                var statsObs = this.WhenAnyValue(x => x.ViewModel.Commit.Stats);
                d(additions.BindText(statsObs.Select(x => x != null ? x.Additions.ToString() : "-")));
                d(deletions.BindText(statsObs.Select(x => x != null ? x.Deletions.ToString() : "-")));

                d(this.WhenAnyValue(x => x.ViewModel.Commit.Parents)
                    .Subscribe(x => parents.Text = x != null ? x.Count.ToString() : "-"));
            });
        }
    }
}


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
using Octokit;

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
            var commentsSection = new Section(null, new TableFooterButton("Add Comment", () => ViewModel.AddCommentCommand.ExecuteIfCan()));

            var additions = split.AddButton("Additions");
            var deletions = split.AddButton("Deletions");
            var parents = split.AddButton("Parents");

            var gotoRepositoryElement = new StringElement(string.Empty) { 
                Font = StringElement.DefaultDetailFont, 
                TextColor = StringElement.DefaultDetailColor,
                Image = Octicon.Repo.ToImage()
            };
            gotoRepositoryElement.Tapped += () => ViewModel.GoToRepositoryCommand.ExecuteIfCan();

            detailsSection.Add(
                new StringElement(Octicon.DiffAdded.ToImage())
                .BindCommand(() => ViewModel.GoToAddedFiles)
                .BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffAdditions).Select(x => x > 0))
                .BindCaption(this.WhenAnyValue(x => x.ViewModel.DiffAdditions).StartWith(0).Select(x => string.Format("{0} added", x))));

            detailsSection.Add(
                new StringElement(Octicon.DiffRemoved.ToImage())
                .BindCommand(() => ViewModel.GoToRemovedFiles)
                .BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffDeletions).Select(x => x > 0))
                .BindCaption(this.WhenAnyValue(x => x.ViewModel.DiffDeletions).StartWith(0).Select(x => string.Format("{0} removed", x))));

            detailsSection.Add(
                new StringElement(Octicon.DiffModified.ToImage())
                .BindCommand(() => ViewModel.GoToModifiedFiles)
                .BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffModifications).Select(x => x > 0))
                .BindCaption(this.WhenAnyValue(x => x.ViewModel.DiffModifications).StartWith(0).Select(x => string.Format("{0} modified", x))));

            detailsSection.Add(new StringElement("All Changes", () => ViewModel.GoToAllFiles.ExecuteIfCan(), Octicon.Diff.ToImage()));

            var commentsElement = new HtmlElement("comments");
            commentsElement.UrlRequested = ViewModel.GoToUrlCommand.ExecuteIfCan;

            ViewModel.Comments.Changed
                .Select(_ => new Unit())
                .StartWith(new Unit())
                .Subscribe(x =>
                {
                    var comments = ViewModel.Comments.Select(c => new Comment(c.Avatar.ToUri(), c.Actor, c.Body, c.UtcCreatedAt.Humanize())).ToList();
                    var commentModel = new CodeHub.WebViews.CommentModel(comments, (int)UIFont.PreferredSubheadline.PointSize);
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

            this.WhenAnyValue(x => x.ViewModel.CommiterName)
                .Subscribe(x => descriptionElement.Caption = x ?? string.Empty);

            this.WhenAnyValue(x => x.ViewModel.CommitMessage)
                .Subscribe(x => descriptionElement.Details = x ?? string.Empty);
            

            this.WhenAnyValue(x => x.ViewModel.RepositoryName)
                .Subscribe(x => gotoRepositoryElement.Caption = x);

            this.WhenAnyValue(x => x.ViewModel.ShowRepository)
                .StartWith(false)
                .Where(x => x)
                .Take(1)
                .Subscribe(x => detailsSection.Add(gotoRepositoryElement));

            this.WhenAnyValue(x => x.ViewModel.Commit)
                .SubscribeSafe(x =>
                    {
                        additions.Text = x != null ? x.Stats.Additions.ToString() : "-";
                        deletions.Text = x != null ? x.Stats.Deletions.ToString() : "-";
                        parents.Text = x != null ? x.Parents.Count.ToString() : "-";
                    });

            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.CommitMessageSummary)
                .Subscribe(x => {
                    HeaderView.Text = x;
                    RefreshHeaderView();
                });

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Subscribe(x => HeaderView.SetImage(x?.ToUri(128), Images.LoginUserUnknown));

            this.WhenAnyValue(x => x.ViewModel.Commit)
                .IsNotNull()
                .Subscribe(x => {
                    HeaderView.SubText = "Commited " + x.Commit.Committer.Date.LocalDateTime.Humanize();
                    RefreshHeaderView();
                });
        }
    }
}


using System;
using UIKit;
using System.Linq;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System.Reactive.Linq;
using GitHubSharp.Models;
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
        private readonly SplitButtonElement _split = new SplitButtonElement();
        private readonly Section _commentsSection;
        private readonly Section _headerSection;
        private readonly Section _detailsSection;
        private readonly MultilinedElement _descriptionElement;
        private readonly StringElement _gotoRepositoryElement;

        public CommitViewController()
        {
            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            var additions = _split.AddButton("Additions");
            var deletions = _split.AddButton("Deletions");
            var parents = _split.AddButton("Parents");
            _headerSection = new Section { _split };

            _descriptionElement = new MultilinedElement();
            _detailsSection = new Section();

            Appeared
                .Take(1)
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.CommitMessage, y => y.ViewModel.CommiterName))
                .Switch()
                .Where(x => x.Item1 != null && x.Item2 != null)
                .Take(1)
                .Subscribe(_ => _detailsSection.Insert(0, UITableViewRowAnimation.Automatic, _descriptionElement));

            this.WhenAnyValue(x => x.ViewModel.CommiterName)
                .Subscribe(x => _descriptionElement.Caption = x ?? string.Empty);

            this.WhenAnyValue(x => x.ViewModel.CommitMessage)
                .Subscribe(x => _descriptionElement.Details = x ?? string.Empty);

            _gotoRepositoryElement = new StringElement(string.Empty) { 
                Font = StringElement.DefaultDetailFont, 
                TextColor = StringElement.DefaultDetailColor,
                Image = Octicon.Repo.ToImage()
            };
            _gotoRepositoryElement.Tapped += () => ViewModel.GoToRepositoryCommand.ExecuteIfCan();

            this.WhenAnyValue(x => x.ViewModel.RepositoryName)
                .Subscribe(x => _gotoRepositoryElement.Caption = x);

            this.WhenAnyValue(x => x.ViewModel.ShowRepository)
                .StartWith(false)
                .Where(x => x)
                .Take(1)
                .Subscribe(x => _detailsSection.Add(_gotoRepositoryElement));

            this.WhenAnyValue(x => x.ViewModel.Commit)
                .SubscribeSafe(x =>
                {
                    additions.Text = x != null ? x.Stats.Additions.ToString() : "-";
                    deletions.Text = x != null ? x.Stats.Deletions.ToString() : "-";
                    parents.Text = x != null ? x.Parents.Count.ToString() : "-";
                });

            this.WhenAnyValue(x => x.ViewModel.CommitMessageSummary)
                .Subscribe(x =>
                {
                    HeaderView.Text = x;
                    RefreshHeaderView();
                });

            this.WhenAnyValue(x => x.ViewModel.Commit)
                .IsNotNull()
                .Subscribe(x =>
                {
                    HeaderView.ImageUri = x.GenerateGravatarUrl();
                    HeaderView.SubText = "Commited " + x.Commit.Committer.Date.LocalDateTime.Humanize();
                    RefreshHeaderView();
                });

            _commentsSection = new Section(null, new TableFooterButton("Add Comment", () => ViewModel.AddCommentCommand.ExecuteIfCan()));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.Image = Images.LoginUserUnknown;

            _detailsSection.Add(
                new StringElement(Octicon.DiffAdded.ToImage())
                .BindCommand(() => ViewModel.GoToAddedFiles)
                .BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffAdditions).Select(x => x > 0))
                .BindCaption(this.WhenAnyValue(x => x.ViewModel.DiffAdditions).StartWith(0).Select(x => string.Format("{0} added", x))));

            _detailsSection.Add(
                new StringElement(Octicon.DiffRemoved.ToImage())
                .BindCommand(() => ViewModel.GoToRemovedFiles)
                .BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffDeletions).Select(x => x > 0))
                .BindCaption(this.WhenAnyValue(x => x.ViewModel.DiffDeletions).StartWith(0).Select(x => string.Format("{0} removed", x))));

            _detailsSection.Add(
                new StringElement(Octicon.DiffModified.ToImage())
                .BindCommand(() => ViewModel.GoToModifiedFiles)
                .BindDisclosure(this.WhenAnyValue(x => x.ViewModel.DiffModifications).Select(x => x > 0))
                .BindCaption(this.WhenAnyValue(x => x.ViewModel.DiffModifications).StartWith(0).Select(x => string.Format("{0} modified", x))));

            _detailsSection.Add(new StringElement("All Changes", () => ViewModel.GoToAllFiles.ExecuteIfCan(), Octicon.Diff.ToImage()));

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
                        _commentsSection.Add(commentsElement);
                    TableView.ReloadData();
                });

            Root.Reset(_headerSection, _detailsSection, _commentsSection);
        }
    }
}


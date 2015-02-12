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
using CodeHub.iOS.ViewComponents;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.Views.Source
{
    public class CommitView : BaseDialogViewController<CommitViewModel>
    {
        private readonly SplitButtonElement _split = new SplitButtonElement();
        private readonly Section _commentsSection;
        private readonly Section _headerSection;

        public CommitView()
        {
            HeaderView.Image = Images.LoginUserUnknown;

            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            var additions = _split.AddButton("Additions");
            var deletions = _split.AddButton("Deletions");
            var parents = _split.AddButton("Parents");
            _headerSection = new Section { _split };

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

            var commentsElement = new HtmlElement("comments");
            commentsElement.UrlRequested = ViewModel.GoToUrlCommand.ExecuteIfCan;

            ViewModel.Comments.Changed
                .Select(_ => new Unit())
                .StartWith(new Unit())
                .Subscribe(x =>
                {
                    var commentModels = ViewModel.Comments.Select(c => new Comment(c.Avatar.ToUri(), c.Actor, c.Body, c.UtcCreatedAt.Humanize()));
                    var razorView = new CommentsView { Model = commentModels };
                    var html = razorView.GenerateString();
                    commentsElement.Value = html;

                    if (commentsElement.GetRootElement() == null && ViewModel.Comments.Count > 0)
                        _commentsSection.Add(commentsElement);
                    TableView.ReloadData();
                });

            ViewModel.WhenAnyValue(x => x.Commit).IsNotNull().Subscribe(commitModel =>
            {
                var detailSection = new Section();
                Root.Reset(_headerSection, detailSection);

                var user = commitModel.GenerateCommiterName();
                detailSection.Add(new MultilinedElement(user, commitModel.Commit.Message)
                {
                    CaptionColor = Theme.MainTextColor,
                    ValueColor = Theme.MainTextColor,
                    BackgroundColor = UIColor.White
                });

                if (ViewModel.ShowRepository)
                {
                    var repo = new StringElement(ViewModel.RepositoryName) { 
                        Font = StringElement.DefaultDetailFont, 
                        TextColor = StringElement.DefaultDetailColor,
                        Image = Octicon.Repo.ToImage()
                    };
                    repo.Tapped += () => ViewModel.GoToRepositoryCommand.Execute(null);
                    detailSection.Add(repo);
                }

                var paths = commitModel.Files.GroupBy(y => {
                    var filename = "/" + y.Filename;
                    return filename.Substring(0, filename.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
                }).OrderBy(y => y.Key);

                foreach (var p in paths)
                {
                    var fileSection = new Section(p.Key);
                    foreach (var x in p)
                    {
                        var y = x;
                        var file = x.Filename.Substring(x.Filename.LastIndexOf('/') + 1);
                        var sse = new ChangesetElement(file, x.Status, x.Additions, x.Deletions);
                        sse.Tapped += () => ViewModel.GoToFileCommand.Execute(y);
                        fileSection.Add(sse);
                    }
                    Root.Add(fileSection);
                }

                Root.Add(_commentsSection);
            });
        }
    }
}


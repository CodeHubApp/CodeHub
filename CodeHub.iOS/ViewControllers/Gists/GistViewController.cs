using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Gists;
using UIKit;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using CodeHub.WebViews;
using Humanizer;
using CodeHub.iOS.DialogElements;
using CodeHub.Core.Utilities;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class GistViewController : BaseDialogViewController<GistViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.Image = Images.LoginUserUnknown;
            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);

            var headerSection = new Section();
            var filesSection = new Section();

            var split = new SplitButtonElement();
            var files = split.AddButton("Files", "-");
            var comments = split.AddButton("Comments", "-");
            var forks = split.AddButton("Forks", "-");
            headerSection.Add(split);

            var footerButton = new TableFooterButton("Add Comment");
            var commentsSection = new Section(null, footerButton);
            var commentsElement = new HtmlElement("comments");
            commentsSection.Add(commentsElement);

            var splitRow1 = new SplitViewElement(Octicon.Lock.ToImage(), Octicon.Package.ToImage());
            var splitRow2 = new SplitViewElement(Octicon.Pencil.ToImage(), Octicon.Star.ToImage());
            var ownerElement = new StringElement("Owner", string.Empty) { Image = Octicon.Person.ToImage() };
            var detailsSection = new Section { splitRow1, splitRow2, ownerElement };
            Root.Reset(headerSection, detailsSection, filesSection, commentsSection);

            Appeared.Take(1).Delay(TimeSpan.FromSeconds(0.35f))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsStarred).Where(x => x.HasValue))
                .Switch()
                .Select(x => x.Value ? Octicon.Star.ToImage() : null)
                .Subscribe(HeaderView.SetSubImage);

            ViewModel.Comments.Changed.Subscribe(_ => {
                var commentModels = ViewModel.Comments
                    .Select(x => {
                        var avatarUrl = x?.User?.AvatarUrl;
                        var avatar = new GitHubAvatar(avatarUrl);
                        return new Comment(avatar.ToUri(), x.User.Login, x.BodyHtml, x.CreatedAt.UtcDateTime.Humanize());
                    }).ToList();

                if (commentModels.Count > 0)
                {
                    var model = new CommentModel(commentModels, (int)UIFont.PreferredSubheadline.PointSize);
                    var razorView = new CommentsView { Model = model };
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

            OnActivation(d => {
                d(footerButton.Clicked.InvokeCommand(ViewModel.AddCommentCommand));
                d(HeaderView.Clicked.InvokeCommand(ViewModel.GoToOwnerCommand));
                d(commentsElement.UrlRequested.InvokeCommand(ViewModel.GoToUrlCommand));
                d(splitRow2.Button2.Clicked.InvokeCommand(ViewModel.ToggleStarCommand));
                d(ownerElement.Clicked.InvokeCommand(ViewModel.GoToOwnerCommand));
                d(DialogSource.SelectedObservable.OfType<FileElement>().Select(x => x.File).InvokeCommand(ViewModel.GoToFileSourceCommand));

                var updatedGistObservable = ViewModel.WhenAnyValue(x => x.Gist).Where(x => x != null);
                d(updatedGistObservable.SubscribeSafe(x => RefreshHeaderView(subtext: x.Description)));
                d(updatedGistObservable.Subscribe(x => filesSection.Reset(x.Files.Select(y => new FileElement(y.Value)))));
                d(ownerElement.BindValue(updatedGistObservable.Select(x => x.Owner?.Login ?? "Anonymous")));
                d(files.BindText(updatedGistObservable.Select(x => x.Files?.Count() ?? 0)));
                d(forks.BindText(updatedGistObservable.Select(x => x.Forks?.Count() ?? 0)));
                d(comments.BindText(updatedGistObservable.Select(x => x.Comments.ToString())));
                d(splitRow1.Button1.BindText(updatedGistObservable.Select(x => x.Public ? "Public" : "Private")));
                d(splitRow1.Button2.BindText(updatedGistObservable.Select(x => "Revisions".ToQuantity(x.History?.Count ?? 0))));

                d(this.WhenAnyValue(x => x.ViewModel.Gist.Owner).Select(x => x == null)
                    .Subscribe(x => ownerElement.Hidden = x));

                d(this.WhenAnyValue(x => x.ViewModel.Avatar)
                    .Subscribe(x => HeaderView.SetImage(x?.ToUri(64), Images.LoginUserUnknown)));

                d(this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Action, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.IsStarred)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value ? "Starred!" : "Unstarred")
                    .Subscribe(x => splitRow2.Button2.Text = x));

                d(splitRow2.Button1.BindText(this.WhenAnyValue(x => x.ViewModel.Gist).Select(x => {
                    var delta = DateTimeOffset.UtcNow.UtcDateTime - x.UpdatedAt.UtcDateTime;
                    return (delta.Days <= 0) ? "Created Today" : string.Format("{0} Days Old", delta.Days);
                })));
            });
        }

        private class FileElement : StringElement
        {
            public Octokit.GistFile File { get; }
            public FileElement(Octokit.GistFile file)
                : base(file.Filename, Octicon.FileCode.ToImage())
            {
                File = file;
                SelectionStyle = UITableViewCellSelectionStyle.Blue;
            }
        }
    }
}


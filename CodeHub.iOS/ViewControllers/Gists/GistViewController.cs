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
        private readonly SplitViewElement _splitRow1;
        private readonly SplitViewElement _splitRow2;
        private readonly StringElement _ownerElement;

        public GistViewController()
        {
            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.GoToOwnerCommand)
                .Select(x => x != null ? new Action(() => ViewModel.GoToOwnerCommand.ExecuteIfCan()) : null)
                .Subscribe(x => HeaderView.ImageButtonAction = x);

            Appeared.Take(1).Delay(TimeSpan.FromSeconds(0.35f))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsStarred).Where(x => x.HasValue))
                .Switch()
                .Select(x => x.Value ? Octicon.Star.ToImage() : null)
                .Subscribe(HeaderView.SetSubImage);

            _splitRow1 = new SplitViewElement();
            _splitRow1.Button1 = new SplitViewElement.SplitButton(Octicon.Lock.ToImage(), string.Empty);
            _splitRow1.Button2 = new SplitViewElement.SplitButton(Octicon.Package.ToImage(), string.Empty);

            _splitRow2 = new SplitViewElement();
            _splitRow2.Button1 = new SplitViewElement.SplitButton(Octicon.Pencil.ToImage(), string.Empty);
            _splitRow2.Button2 = new SplitViewElement.SplitButton(Octicon.Star.ToImage(), string.Empty, () => ViewModel.ToggleStarCommand.ExecuteIfCan());

            _ownerElement = new StringElement("Owner", string.Empty) 
            { 
                Image = Octicon.Person.ToImage(),
                ImageTintColor = Theme.PrimaryNavigationBarColor
            };

            this.WhenAnyValue(x => x.ViewModel.IsStarred)
                .Where(x => x.HasValue)
                .Select(x => x.Value ? "Starred!" : "Unstarred")
                .Subscribe(x => _splitRow2.Button2.Text = x);

            this.WhenAnyValue(x => x.ViewModel.Gist)
                .IsNotNull()
                .SubscribeSafe(x =>
                {
                    var revisionCount = x.History == null ? 0 : x.History.Count;

                    _splitRow1.Button1.Text = x.Public ? "Public" : "Private";
                    _splitRow1.Button2.Text = revisionCount + " Revisions";

                    var delta = DateTimeOffset.UtcNow.UtcDateTime - x.UpdatedAt.UtcDateTime;
                    if (delta.Days <= 0)
                        _splitRow2.Button1.Text = "Created Today";
                    else
                        _splitRow2.Button1.Text = string.Format("{0} Days Old", delta.Days);
                });
        }

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

            var commentsSection = new Section() { FooterView = new TableFooterButton("Add Comment", ViewModel.AddCommentCommand.ExecuteIfCan) };
            var commentsElement = new HtmlElement("comments");
            commentsElement.UrlRequested = ViewModel.GoToUrlCommand.ExecuteIfCan;
            commentsSection.Add(commentsElement);

            var detailsSection = new Section { _splitRow1, _splitRow2 };

            Root.Reset(headerSection, detailsSection, filesSection, commentsSection);

            var updatedGistObservable = ViewModel.WhenAnyValue(x => x.Gist).Where(x => x != null);

            ViewModel.WhenAnyValue(x => x.Gist)
                .IsNotNull()
                .Select(x => x.Owner)
                .Subscribe(x =>
                {
                    if (x == null)
                        detailsSection.Remove(_ownerElement);
                    else if (x != null && !detailsSection.Contains(_ownerElement))
                        detailsSection.Add(_ownerElement);
                });


            updatedGistObservable
                .Select(x => x?.Owner?.Login ?? "Anonymous")
                .Subscribe(x => _ownerElement.Value = x);

            updatedGistObservable
                .Select(x => x.Owner != null ? () => ViewModel.GoToOwnerCommand.ExecuteIfCan() : (Action)null)
                .SubscribeSafe(x => _ownerElement.Tapped = x);

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Subscribe(x => HeaderView.SetImage(x?.ToUri(64), Images.LoginUserUnknown));

            updatedGistObservable.SubscribeSafe(x => {
                HeaderView.SubText = x.Description;
                RefreshHeaderView();
            });

            updatedGistObservable.Select(x => x.Files == null ? 0 : x.Files.Count()).SubscribeSafe(x => files.Text = x.ToString());
            updatedGistObservable.SubscribeSafe(x => comments.Text = x.Comments.ToString());
            updatedGistObservable.Select(x => x.Forks == null ? 0 : x.Forks.Count()).SubscribeSafe(x => forks.Text = x.ToString());

            updatedGistObservable.Subscribe(x =>
            {
                var elements = new List<Element>();
                foreach (var file in x.Files.Keys)
                {
                    var sse = new StringElement(file);
                    sse.Image = Octicon.FileCode.ToImage();
                    sse.Tapped += () => ViewModel.GoToFileSourceCommand.Execute(x.Files[file]);
                    elements.Add(sse);
                }

                filesSection.Reset(elements);
            });


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
        }
    }
}


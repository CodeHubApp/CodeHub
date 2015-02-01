using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Gists;
using UIKit;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using CodeHub.WebViews;
using Humanizer;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Gists
{
    public class GistView : BaseDialogViewController<GistViewModel>
    {
        private readonly SplitViewElement _splitRow1;
        private readonly SplitViewElement _splitRow2;
        private readonly StyledStringElement _ownerElement;

        public GistView()
        {
            HeaderView.Image = Images.LoginUserUnknown;
            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);

            this.WhenViewModel(x => x.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.GoToOwnerCommand)
                .Select(x => x != null ? new Action(() => ViewModel.GoToOwnerCommand.ExecuteIfCan()) : null)
                .Subscribe(x => HeaderView.ImageButtonAction = x);

            Appeared.Take(1).Delay(TimeSpan.FromSeconds(0.35f))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsStarred).Where(x => x.HasValue))
                .Switch()
                .Select(x => x.Value ? Images.Star.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) : null)
                .Subscribe(HeaderView.SetSubImage);

            _splitRow1 = new SplitViewElement();
            _splitRow1.Button1 = new SplitViewElement.SplitButton(Images.Lock, string.Empty);
            _splitRow1.Button2 = new SplitViewElement.SplitButton(Images.Package, string.Empty);

            _splitRow2 = new SplitViewElement();
            _splitRow2.Button1 = new SplitViewElement.SplitButton(Images.Pencil, string.Empty);
            _splitRow2.Button2 = new SplitViewElement.SplitButton(Images.Star, string.Empty, () => ViewModel.ToggleStarCommand.ExecuteIfCan());

            _ownerElement = new StyledStringElement("Owner", string.Empty) { Image = Images.Person };
            _ownerElement.Tapped += () => ViewModel.GoToOwnerCommand.ExecuteIfCan();

            this.WhenAnyValue(x => x.ViewModel.IsStarred)
                .Where(x => x.HasValue)
                .Select(x => x.Value ? "Starred!" : "Unstarred")
                .Subscribe(x => _splitRow2.Button2.Text = x);

            this.WhenAnyValue(x => x.ViewModel.Gist)
                .IsNotNull()
                .SubscribeSafe(x =>
                {
                    var publicGist = x.Public.HasValue && x.Public.Value;
                    var revisionCount = x.History == null ? 0 : x.History.Count;

                    _splitRow1.Button1.Text = publicGist ? "Public" : "Private";
                    _splitRow1.Button1.Image = publicGist ? Images.Lock : Images.Lock;
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

            var headerSection = new Section();
            var filesSection = new Section("Files");

            var split = new SplitButtonElement();
            var files = split.AddButton("Files", "-");
            var comments = split.AddButton("Comments", "-");
            var forks = split.AddButton("Forks", "-");
            headerSection.Add(split);

            var commentsSection = new Section("Comments") { FooterView = new TableFooterButton("Add Comment", ViewModel.AddCommentCommand.ExecuteIfCan) };
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

            updatedGistObservable.SubscribeSafe(x =>
            {
                if (x.Owner == null)
                {
                    _ownerElement.Value = "Anonymous";
                    _ownerElement.Accessory = UITableViewCellAccessory.None;
                }
                else
                {
                    _ownerElement.Value = x.Owner.Login;
                    _ownerElement.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                }

                Root.Reload(_ownerElement);
            });

            updatedGistObservable.SubscribeSafe(x =>
            {
                HeaderView.SubText = x.Description;
                if (x.Owner != null) 
                    HeaderView.ImageUri = x.Owner.AvatarUrl;
                else
                    HeaderView.Image = Images.LoginUserUnknown;
                TableView.ReloadData();
            });

            updatedGistObservable.Select(x => x.Files == null ? 0 : x.Files.Count()).SubscribeSafe(x => files.Text = x.ToString());
            updatedGistObservable.SubscribeSafe(x => comments.Text = x.Comments.ToString());
            updatedGistObservable.Select(x => x.Forks == null ? 0 : x.Forks.Count()).SubscribeSafe(x => forks.Text = x.ToString());

            updatedGistObservable.Subscribe(x =>
            {
                var elements = new List<Element>();
                foreach (var file in x.Files.Keys)
                {
                    var sse = new StyledStringElement(file, x.Files[file].Size + " bytes", UITableViewCellStyle.Subtitle) { 
                        Accessory = UITableViewCellAccessory.DisclosureIndicator, 
                        LineBreakMode = UILineBreakMode.TailTruncation,
                        Lines = 1 
                    };

                    sse.Tapped += () => ViewModel.GoToFileSourceCommand.Execute(x.Files[file]);
                    elements.Add(sse);
                }

                filesSection.Reset(elements);
            });


            ViewModel.Comments.Changed.Subscribe(_ =>
            {
                var commentModels = ViewModel.Comments
                    .Select(x => new Comment(x.User.AvatarUrl, x.User.Login, x.BodyHtml, x.CreatedAt.UtcDateTime.Humanize()))
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
        }
    }
}


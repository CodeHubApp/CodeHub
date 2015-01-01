using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Gists;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.DialogElements;
using System.Collections.Generic;
using System.Linq;
using CodeHub.WebViews;
using Humanizer;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Gists
{
    public class GistView : BaseDialogViewController<GistViewModel>
    {
        public GistView()
        {
            this.WhenViewModel(x => x.ShowMenuCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = x.ToBarButtonItem(UIBarButtonSystemItem.Action));

            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);
            this.WhenAnyValue(x => x.ViewModel.GoToOwnerCommand).Subscribe(x => 
                HeaderView.ImageButtonAction = x != null ? new Action(() => ViewModel.GoToOwnerCommand.ExecuteIfCan()) : null);

            Appeared.Take(1).Delay(TimeSpan.FromSeconds(0.35f)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
                this.WhenAnyValue(x => x.ViewModel.IsStarred).Where(x => x.HasValue).Subscribe(x => 
                    HeaderView.SetSubImage(x.Value ? Images.Star.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) : null)));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.Image = Images.LoginUserUnknown;

            var headerSection = new Section();
            var filesSection = new Section("Files");

            var split = new SplitButtonElement();
            var files = split.AddButton("Files", "-");
            var comments = split.AddButton("Comments", "-");
            var forks = split.AddButton("Forks", "-");
            headerSection.Add(split);

            var commentsSection = new Section("Comments") { FooterView = new TableFooterButton("Add Comment", ViewModel.AddCommentCommand.ExecuteIfCan) };
            var commentsElement = new WebElement("comments");
            commentsElement.UrlRequested = ViewModel.GoToUrlCommand.ExecuteIfCan;
            commentsSection.Add(commentsElement);

            var detailsSection = new Section();
            var splitElement1 = new SplitElement();
            splitElement1.Button1 = new SplitElement.SplitButton(Images.Lock, string.Empty);
            splitElement1.Button2 = new SplitElement.SplitButton(Images.Package, string.Empty);
            detailsSection.Add(splitElement1);

            var splitElement2 = new SplitElement();
            splitElement2.Button1 = new SplitElement.SplitButton(Images.Calendar, string.Empty);
            splitElement2.Button2 = new SplitElement.SplitButton(Images.Star, string.Empty, ViewModel.ToggleStarCommand.ExecuteIfCan);
            detailsSection.Add(splitElement2);

            var owner = new StyledStringElement("Owner", string.Empty) { Image = Images.Person };
            owner.Tapped += () => ViewModel.GoToOwnerCommand.ExecuteIfCan();

            Root.Reset(headerSection, detailsSection, filesSection, commentsSection);

            var updatedGistObservable = ViewModel.WhenAnyValue(x => x.Gist).Where(x => x != null);

            ViewModel.WhenAnyValue(x => x.Gist).IsNotNull().Select(x => x.Owner).Subscribe(x =>
            {
                if (x == null)
                    detailsSection.Remove(owner);
                else if (x != null && !detailsSection.Contains(owner))
                    detailsSection.Add(owner);
            });

            updatedGistObservable.SubscribeSafe(x =>
            {
                var publicGist = x.Public.HasValue && x.Public.Value;
                var revisionCount = x.History == null ? 0 : x.History.Count;

                splitElement1.Button1.Text = publicGist ? "Public" : "Private";
                splitElement1.Button1.Image = publicGist ? Images.Lock : Images.Lock;
                splitElement1.Button2.Text = revisionCount + " Revisions";
                splitElement2.Button1.Text = x.UpdatedAt.ToLocalTime().ToString("MM/dd/yy");
            });

            updatedGistObservable.SubscribeSafe(x =>
            {
                if (x.Owner == null)
                {
                    owner.Value = "Anonymous";
                    owner.Accessory = UITableViewCellAccessory.None;
                }
                else
                {
                    owner.Value = x.Owner.Login;
                    owner.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                }

                Root.Reload(owner);
            });

            ViewModel.WhenAnyValue(x => x.IsStarred).Where(x => x.HasValue).Subscribe(x =>
            {
                splitElement2.Button2.Text = x.Value ? "Starred!" : "Unstarred";
                splitElement2.Button2.Image = x.Value ? Images.Star : Images.Star;
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


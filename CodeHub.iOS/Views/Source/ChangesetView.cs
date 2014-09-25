using System;
using CodeFramework.Elements;
using MonoTouch.UIKit;
using System.Linq;
using MonoTouch.Foundation;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System.Reactive.Linq;
using GitHubSharp.Models;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetView : ViewModelPrettyDialogViewController<ChangesetViewModel>
    {
        private SplitButtonElement _split;
        private UIActionSheet _actionSheet;

        public ChangesetView()
        {
            Title = "Commit";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.Commit).Select(x => x != null));

            _split = new SplitButtonElement();
            var additions = _split.AddButton("Additions", "-");
            var deletions = _split.AddButton("Deletions", "-");
            var parents = _split.AddButton("Parents", "-");

            var headerSection = new Section(HeaderView) { _split };
            Root.Reset(headerSection);

            ViewModel.WhenAnyValue(x => x.Commit).IsNotNull().Subscribe(x =>
            {
                HeaderView.Image = Images.LoginUserUnknown;

                if (x.Author != null)
                {
                    HeaderView.Text = x.Author.Login;
                    HeaderView.ImageUri = x.Author.AvatarUrl;
                }
                else
                {
                    HeaderView.Text = "Unknown Author";
                }

                HeaderView.SubText = "Commited " + (x.Commit.Committer.Date).ToDaysAgo();

                additions.Text = x.Stats.Additions.ToString();
                deletions.Text = x.Stats.Deletions.ToString();
                parents.Text = x.Parents.Count.ToString();

                ReloadData();
            });

            ViewModel.WhenAnyValue(x => x.Commit).Where(x => x != null).Subscribe(Render);

            //ViewModel.BindCollection(x => x.Comments, a => Render());
        }

        public void Render(CommitModel commitModel)
        {
            var headerSection = new Section(HeaderView) { _split };
            var detailSection = new Section();
            Root.Reset(headerSection, detailSection);

            var user = "Unknown";
            if (commitModel.Commit.Author != null)
                user = commitModel.Commit.Author.Name;
            if (commitModel.Commit.Committer != null)
                user = commitModel.Commit.Committer.Name;

            detailSection.Add(new MultilinedElement(user, commitModel.Commit.Message)
            {
                CaptionColor = Theme.CurrentTheme.MainTextColor,
                ValueColor = Theme.CurrentTheme.MainTextColor,
                BackgroundColor = UIColor.White
            });

            if (ViewModel.ShowRepository)
            {
                var repo = new StyledStringElement(ViewModel.RepositoryName) { 
                    Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator, 
                    Lines = 1, 
                    Font = StyledStringElement.DefaultDetailFont, 
                    TextColor = StyledStringElement.DefaultDetailColor,
                    Image = Images.Repo
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
//
//			var fileSection = new Section();
//            commitModel.Files.ForEach(x => {
//                var file = x.Filename.Substring(x.Filename.LastIndexOf('/') + 1);
//                var sse = new ChangesetElement(file, x.Status, x.Additions, x.Deletions);
//                sse.Tapped += () => ViewModel.GoToFileCommand.Execute(x);
//                fileSection.Add(sse);
//            });

//            if (fileSection.Elements.Count > 0)
//                root.Add(fileSection);
//

			var commentSection = new Section();
            foreach (var comment in ViewModel.Comments)
            {
                //The path should be empty to indicate it's a comment on the entire commit, not a specific file
                if (!string.IsNullOrEmpty(comment.Path))
                    continue;

                commentSection.Add(new CommentElement {
                    Name = comment.User.Login,
                    Time = comment.CreatedAt.ToDaysAgo(),
                    String = comment.Body,
                    Image = Images.Anonymous,
                    ImageUri = new Uri(comment.User.AvatarUrl),
                    BackgroundColor = UIColor.White,
                });
            }

			if (commentSection.Count > 0)
				Root.Add(commentSection);

            var addComment = new StyledStringElement("Add Comment") { Image = Images.Pencil };
            addComment.Tapped += AddCommentTapped;
            Root.Add(new Section { addComment });
        }

        void AddCommentTapped()
        {
//            var composer = new MarkdownComposerViewController();
//			composer.NewComment(this, async (text) => {
//                try
//                {
//					await composer.DoWorkAsync("Commenting...", () => ViewModel.AddComment(text));
//					composer.CloseComposer();
//                }
//                catch (Exception e)
//                {
//					MonoTouch.Utilities.ShowAlert("Unable to post comment!", e.Message);
//                }
//                finally
//                {
//                    composer.EnableSendButton = true;
//                }
//            });
        }

		private void ShowExtraMenu()
		{
            _actionSheet = new UIActionSheet(Title);
            var addComment = _actionSheet.AddButton("Add Comment");
            var copySha = _actionSheet.AddButton("Copy Sha");
            var shareButton = _actionSheet.AddButton("Share");
			//var showButton = sheet.AddButton("Show in GitHub");
            var cancelButton = _actionSheet.AddButton("Cancel");
            _actionSheet.CancelButtonIndex = cancelButton;
            _actionSheet.DismissWithClickedButtonIndex(cancelButton, true);
            _actionSheet.Clicked += (s, e) => 
			{
				try
				{
					// Pin to menu
					if (e.ButtonIndex == addComment)
					{
						AddCommentTapped();
					}
					else if (e.ButtonIndex == copySha)
					{
						UIPasteboard.General.String = ViewModel.Commit.Sha;
					}
					else if (e.ButtonIndex == shareButton)
					{
						var item = new NSUrl(ViewModel.Commit.Url);
						var activityItems = new MonoTouch.Foundation.NSObject[] { item };
						UIActivity[] applicationActivities = null;
						var activityController = new UIActivityViewController (activityItems, applicationActivities);
						PresentViewController (activityController, true, null);
					}
	//				else if (e.ButtonIndex == showButton)
	//				{
	//					ViewModel.GoToHtmlUrlCommand.Execute(null);
	//				}
				}
				catch
				{
				}

			    _actionSheet = null;
			};

            _actionSheet.ShowInView(this.View);
		}
    }
}


using System;
using CodeFramework.Elements;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;
using UIKit;
using CodeFramework.iOS.Utils;
using System.Linq;
using Foundation;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetView : ViewModelDrivenDialogViewController
    {
        private readonly HeaderView _header = new HeaderView();

        public new ChangesetViewModel ViewModel 
        {
            get { return (ChangesetViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
        }
        
        public ChangesetView()
        {
            Title = "Commit".t();
            Root.UnevenRows = true;
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _header.Title = "Commit: ".t() + ViewModel.Node.Substring(0, ViewModel.Node.Length > 10 ? 10 : ViewModel.Node.Length);
            ViewModel.Bind(x => x.Changeset, Render);
            ViewModel.BindCollection(x => x.Comments, a => Render());
        }

        public void Render()
        {
            var commitModel = ViewModel.Changeset;
            if (commitModel == null)
                return;

            var root = new RootElement(Title) { UnevenRows = Root.UnevenRows };

            _header.Subtitle = "Commited ".t() + (commitModel.Commit.Committer.Date).ToDaysAgo();
            var headerSection = new Section(_header);
            root.Add(headerSection);

            var detailSection = new Section();
            root.Add(detailSection);

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
                var repo = new StyledStringElement(ViewModel.Repository) { 
                    Accessory = UIKit.UITableViewCellAccessory.DisclosureIndicator, 
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
				root.Add(fileSection);
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

			if (commentSection.Elements.Count > 0)
				root.Add(commentSection);

            var addComment = new StyledStringElement("Add Comment".t()) { Image = Images.Pencil };
            addComment.Tapped += AddCommentTapped;
			root.Add(new Section { addComment });
            Root = root; 
        }

        void AddCommentTapped()
        {
            var composer = new MarkdownComposerViewController();
			composer.NewComment(this, async (text) => {
                try
                {
					await composer.DoWorkAsync("Commenting...".t(), () => ViewModel.AddComment(text));
					composer.CloseComposer();
                }
                catch (Exception e)
                {
					MonoTouch.Utilities.ShowAlert("Unable to post comment!", e.Message);
                }
                finally
                {
                    composer.EnableSendButton = true;
                }
            });
        }

		private void ShowExtraMenu()
		{
			var changeset = ViewModel.Changeset;
			if (changeset == null)
				return;

			var sheet = MonoTouch.Utilities.GetSheet(Title);
			var addComment = sheet.AddButton("Add Comment".t());
			var copySha = sheet.AddButton("Copy Sha".t());
			var shareButton = sheet.AddButton("Share".t());
			//var showButton = sheet.AddButton("Show in GitHub".t());
			var cancelButton = sheet.AddButton("Cancel".t());
			sheet.CancelButtonIndex = cancelButton;
			sheet.DismissWithClickedButtonIndex(cancelButton, true);
			sheet.Dismissed += (s, e) => 
			{
				BeginInvokeOnMainThread(() =>
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
						UIPasteboard.General.String = ViewModel.Changeset.Sha;
					}
					else if (e.ButtonIndex == shareButton)
					{
						var item = new NSUrl(ViewModel.Changeset.Url);
						var activityItems = new Foundation.NSObject[] { item };
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
					});
			};

			sheet.ShowInView(this.View);
		}
    }
}


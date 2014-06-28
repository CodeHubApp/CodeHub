using System;
using CodeFramework.Elements;
using CodeFramework.iOS.ViewComponents;
using CodeFramework.iOS.Views;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Linq;
using MonoTouch.Foundation;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System.Reactive.Linq;
using CodeStash.iOS.Views;
using GitHubSharp.Models;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetView : ViewModelDialogView<ChangesetViewModel>
    {
        private ImageAndTitleHeaderView _header;
        private UIActionSheet _actionSheet;
        private SplitButtonElement _split;

        public ChangesetView()
            : base(title: "Commit")
        {
            Root.UnevenRows = true;
        }

        public override void ViewDidLoad()
        {

            base.ViewDidLoad();

            NavigationController.NavigationBar.ShadowImage = new UIImage();

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.Commit, x => x != null));

            TableView.SectionHeaderHeight = 0;
            RefreshControl.TintColor = UIColor.LightGray;

            _header = new ImageAndTitleHeaderView 
            { 
                BackgroundColor = NavigationController.NavigationBar.BackgroundColor,
                TextColor = UIColor.White,
                SubTextColor = UIColor.LightGray,
                ImageTint = UIColor.White
            };

            this.CreateTopBackground(_header.BackgroundColor);

            _split = new SplitButtonElement();
            var additions = _split.AddButton("Additions", "-");
            var deletions = _split.AddButton("Deletions", "-");
            var parents = _split.AddButton("Parents", "-");

            ViewModel.WhenAnyValue(x => x.Commit).Where(x => x != null).Subscribe(x =>
            {
                _header.Image = Images.LoginUserUnknown;

                if (x.Author != null)
                {
                    _header.Text = x.Author.Login;
                    _header.ImageUri = x.Author.AvatarUrl;
                }
                else
                {
                    _header.Text = "Unknown Author";
                }

                _header.SubText = "Commited " + (x.Commit.Committer.Date).ToDaysAgo();

                additions.Text = x.Stats.Additions.ToString();
                deletions.Text = x.Stats.Deletions.ToString();
                parents.Text = x.Parents.Count.ToString();
            });

            ViewModel.WhenAnyValue(x => x.Commit).Where(x => x != null).Subscribe(Render);

            //ViewModel.BindCollection(x => x.Comments, a => Render());
        }

        protected override void Scrolled(System.Drawing.PointF point)
        {
            if (point.Y > 0)
            {
                NavigationController.NavigationBar.ShadowImage = null;
            }
            else
            {
                if (NavigationController.NavigationBar.ShadowImage == null)
                    NavigationController.NavigationBar.ShadowImage = new UIImage();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.NavigationBar.ShadowImage = null;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.ShadowImage = new UIImage();
        }

        public void Render(CommitModel commitModel)
        {
            var root = new RootElement(Title) { UnevenRows = Root.UnevenRows };

            var headerSection = new Section(_header) { _split };
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

            var addComment = new StyledStringElement("Add Comment") { Image = Images.Pencil };
            addComment.Tapped += AddCommentTapped;
			root.Add(new Section { addComment });
            Root = root; 
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


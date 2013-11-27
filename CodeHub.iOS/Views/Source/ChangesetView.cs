using System;
using CodeFramework.Elements;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetView : ViewModelDrivenViewController
    {
        private readonly HeaderView _header = new HeaderView();
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;

        public new ChangesetViewModel ViewModel 
        {
            get { return (ChangesetViewModel)base.ViewModel; }
            protected set { base.ViewModel = value; }
        }
        
        public ChangesetView()
        {
            Title = "Commit".t();
            Root.UnevenRows = true;
			_viewSegment = new UISegmentedControl(new object[] { "Changes".t(), "Comments".t() });
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Fucking bug in the divider
            BeginInvokeOnMainThread(delegate {
                _viewSegment.SelectedSegment = 1;
                _viewSegment.SelectedSegment = 0;
                _viewSegment.ValueChanged += (sender, e) => Render();
            });

            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
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
            if (commitModel.Author != null)
                user = commitModel.Author.Login;
            if (commitModel.Commit.Author != null)
                user = commitModel.Commit.Author.Name;

            detailSection.Add(new MultilinedElement(user, commitModel.Commit.Message)
            {
                CaptionColor = Theme.CurrentTheme.MainTextColor,
                ValueColor = Theme.CurrentTheme.MainTextColor,
                BackgroundColor = UIColor.White
            });

            if (ViewModel.ShowRepository)
            {
                var repo = new StyledStringElement(ViewModel.Repository) { 
                    Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator, 
                    Lines = 1, 
                    Font = StyledStringElement.DefaultDetailFont, 
                    TextColor = StyledStringElement.DefaultDetailColor,
                    Image = Images.Repo
                };
                repo.Tapped += () => ViewModel.GoToRepositoryCommand.Execute(null);
                detailSection.Add(repo);
            }

            if (_viewSegment.SelectedSegment == 0)
            {
                var fileSection = new Section();
                commitModel.Files.ForEach(x => {
                    var file = x.Filename.Substring(x.Filename.LastIndexOf('/') + 1);
                    var sse = new ChangesetElement(file, x.Status, x.Additions, x.Deletions);
                    sse.Tapped += () => {
                        string parent = null;
                        if (commitModel.Parents != null && commitModel.Parents.Count > 0)
                            parent = commitModel.Parents[0].Sha;

                        // This could mean it's a binary or it's just been moved with no changes...
//                        if (x.Patch == null)
//                            NavigationController.PushViewController(new RawContentViewController(x.RawUrl, x.BlobUrl), true);
//                        else
//                            NavigationController.PushViewController(new ChangesetDiffViewController(ViewModel.User, ViewModel.Repository, commitModel.Sha, x) { Comments = ViewModel.Comments.Items.ToList() }, true);
                    };
                    fileSection.Add(sse);
                });

                if (fileSection.Elements.Count > 0)
                    root.Add(fileSection);
            }
            else if (_viewSegment.SelectedSegment == 1)
            {
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
            }

            Root = root; 
        }

        void AddCommentTapped()
        {
//            var composer = new Composer();
//            composer.NewComment(this, (text) => {
//                try
//                {
//                    composer.DoWorkTest("Commenting...".t(), async () => {
//                        await ViewModel.AddComment(text);
//                        composer.CloseComposer();
//                    });
//                }
//                catch (Exception e)
//                {
//                    Utilities.ShowAlert("Unable to post comment!", e.Message);
//                }
//                finally
//                {
//                    composer.EnableSendButton = true;
//                }
//            });
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}


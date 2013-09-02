using System;
using CodeFramework.Controllers;
using CodeHub.Controllers;
using CodeFramework.Views;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch;
using System.Linq;
using CodeFramework.Elements;

namespace CodeHub.ViewControllers
{
    public class PullRequestViewController : BaseControllerDrivenViewController, IView<PullRequestController.ViewModel>
    {
        private readonly HeaderView _header;
        private readonly SplitElement _split1;

        private bool _scrollToLastComment;

        public new PullRequestController Controller
        {
            get { return (PullRequestController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public PullRequestViewController(string user, string slug, long id)
        {
            Title = "Pull Request".t();
            Controller = new PullRequestController(this, user, slug, id);

            Root.UnevenRows = true;
            _header = new HeaderView(View.Bounds.Width) { ShadowImage = false };
            _split1 = new SplitElement(new SplitElement.Row { Image1 = Images.Buttons.Cog, Image2 = Images.Milestone }) { BackgroundColor = UIColor.White };
        }

        public void Render(PullRequestController.ViewModel model)
        {
            var root = new RootElement(model.PullRequest.Title);
            _header.Title = Title;
            _header.Subtitle = "Updated " + (model.PullRequest.UpdatedAt).ToDaysAgo();
            _header.SetNeedsDisplay();
            root.Add(new Section(_header));

            var secDetails = new Section();
            if (!string.IsNullOrEmpty(model.PullRequest.Body))
            {
                var desc = new MultilinedElement(model.PullRequest.Body.Trim()) { BackgroundColor = UIColor.White };
                desc.CaptionFont = desc.ValueFont;
                desc.CaptionColor = desc.ValueColor;
                secDetails.Add(desc);
            }

            _split1.Value.Text1 = model.PullRequest.State;
            _split1.Value.Text2 = (model.PullRequest.Merged == null || !model.PullRequest.Merged.Value) ? "Not Merged" : "Merged";
            secDetails.Add(_split1);
            root.Add(secDetails);

            root.Add(new Section {
                new StyledStringElement("Commits", () => NavigationController.PushViewController(new ChangesetViewController(Controller.User, Controller.Repo, Controller.PullRequestId), true), Images.Changes),
                new StyledStringElement("Files", () => NavigationController.PushViewController(new PullRequestFilesViewController(Controller.User, Controller.Repo, Controller.PullRequestId), true), Images.File),
            });

            if (model.Comments.Count > 0)
            {
                var commentsSec = new Section();
                model.Comments.OrderBy(x => (x.CreatedAt)).ToList().ForEach(x => {
                    if (!string.IsNullOrEmpty(x.Body))
                        commentsSec.Add(new CommentElement {
                            Name = x.User.Login,
                            Time = x.CreatedAt.ToDaysAgo(),
                            String = x.Body,
                            Image = CodeFramework.Images.Misc.Anonymous,
                            ImageUri = new Uri(x.User.AvatarUrl),
                            BackgroundColor = UIColor.White,
                        });
                });

                //Load more if there's more comments
//                if (model.MoreComments != null)
//                {
//                    var loadMore = new PaginateElement("Load More".t(), "Loading...".t(), 
//                                                       e => this.DoWorkNoHud(() => model.MoreComments(),
//                                          x => Utilities.ShowAlert("Unable to load more!".t(), x.Message))) { AutoLoadOnVisible = false, Background = false };
//                    commentsSec.Add(loadMore);
//                }

                if (commentsSec.Elements.Count > 0)
                    root.Add(commentsSec);
            }


            var addComment = new StyledStringElement("Add Comment") { Image = Images.Pencil };
            addComment.Tapped += AddCommentTapped;
            root.Add(new Section { addComment });
            Root = root;

            //            if (_scrollToLastComment && _comments.Elements.Count > 0)
            //            {
            //                TableView.ScrollToRow(NSIndexPath.FromRowSection(_comments.Elements.Count - 1, 2), UITableViewScrollPosition.Top, true);
            //                _scrollToLastComment = false;
            //            }
        }

        void AddCommentTapped()
        {
            var composer = new Composer();
            composer.NewComment(this, () => {
                var text = composer.Text;
                composer.DoWork(() => {
                    Controller.AddComment(text);
                    InvokeOnMainThread(() => {
                        composer.CloseComposer();
                        _scrollToLastComment = true;
                    });
                }, ex =>
                {
                    Utilities.ShowAlert("Unable to post comment!", ex.Message);
                    composer.EnableSendButton = true;
                });
            });
        }

        public override UIView InputAccessoryView
        {
            get
            {
                var u = new UIView(new System.Drawing.RectangleF(0, 0, 320f, 27)) { BackgroundColor = UIColor.White };
                return u;
            }
        }
    }
}


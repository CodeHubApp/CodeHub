using System;
using CodeFramework.Controllers;
using CodeHub.Controllers;
using CodeFramework.Views;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch;
using System.Collections.Generic;
using System.Linq;
using CodeFramework.Elements;
using MonoTouch.Foundation;
using GitHubSharp.Models;

namespace CodeHub.ViewControllers
{
    public class IssueViewController : BaseControllerDrivenViewController, IView<IssueInfoController.ViewModel>
    {
        public long Id { get; private set; }
        public string User { get; private set; }
        public string Slug { get; private set; }

        private readonly HeaderView _header;
        private readonly SplitElement _split1;

        private bool _scrollToLastComment;
        private bool _issueRemoved;

        public new IssueInfoController Controller
        {
            get { return (IssueInfoController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public IssueViewController(string user, string slug, long id)
        {
            User = user;
            Slug = slug;
            Id = id;
            Title = "Issue #" + id;
            Controller = new IssueInfoController(this, user, slug, id);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(CodeFramework.Images.Buttons.Edit, () => {
//                var m = Controller.Model;
//                var editController = new IssueEditViewController {
//                    ExistingIssue = m.Issue,
//                    Username = User,
//                    RepoSlug = Slug,
//                    Title = "Edit Issue",
//                    Success = EditingComplete,
//                };
//                NavigationController.PushViewController(editController, true);
            }));
            NavigationItem.RightBarButtonItem.Enabled = false;

            Style = UITableViewStyle.Grouped;
            Root.UnevenRows = true;
            _header = new HeaderView(View.Bounds.Width) { ShadowImage = false };
            _split1 = new SplitElement(new SplitElement.Row { Image1 = Images.Cog, Image2 = Images.Milestone }) { BackgroundColor = UIColor.White };
        }

        public void Render(IssueInfoController.ViewModel model)
        {
            //This means we've deleted it. Due to the code flow, render will get called after the update, regardless.
            if (model == null || model.Issue == null)
                return;

            //We've loaded, we can edit
            NavigationItem.RightBarButtonItem.Enabled = true;


            var root = new RootElement(Title);
            _header.Title = model.Issue.Title;
            _header.Subtitle = "Updated " + (model.Issue.UpdatedAt).ToDaysAgo();
            _header.SetNeedsDisplay();
            root.Add(new Section(_header));

            var secDetails = new Section();
            if (!string.IsNullOrEmpty(model.Issue.Body))
            {
                var desc = new MultilinedElement(model.Issue.Body.Trim()) { BackgroundColor = UIColor.White };
                desc.CaptionFont = desc.ValueFont;
                desc.CaptionColor = desc.ValueColor;
                secDetails.Add(desc);
            }

            _split1.Value.Text1 = model.Issue.State;
            _split1.Value.Text2 = model.Issue.Milestone == null ? "No Milestone".t() : model.Issue.Milestone.Title;
            secDetails.Add(_split1);

            var responsible = new StyledStringElement(model.Issue.Assignee != null ? model.Issue.Assignee.Login : "Unassigned".t()) {
                Font = StyledStringElement.DefaultDetailFont,
                TextColor = StyledStringElement.DefaultDetailColor,
                Image = Images.Person
            };
            if (model.Issue.Assignee != null)
            {
                responsible.Tapped += () => NavigationController.PushViewController(new ProfileViewController(model.Issue.Assignee.Login), true);
                responsible.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }

            secDetails.Add(responsible);
            root.Add(secDetails);

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
                if (model.MoreComments != null)
                {
                    var loadMore = new PaginateElement("Load More".t(), "Loading...".t(), 
                                                       e => this.DoWorkNoHud(() => model.MoreComments(),
                                                       x => Utilities.ShowAlert("Unable to load more!".t(), x.Message))) { AutoLoadOnVisible = false, Background = false };
                    commentsSec.Add(loadMore);
                }

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

        void EditingComplete(IssueModel model)
        {
            Controller.EditComplete(model);

            //If it's null then we've deleted it!
            if (model == null)
                _issueRemoved = true;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (_issueRemoved)
                NavigationController.PopViewControllerAnimated(true);
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


using System;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Issues;
using GitHubSharp.Models;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FileSourceViewController = CodeHub.iOS.Views.Source.FileSourceViewController;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueView : WebViewController
    {
        private bool _issueRemoved;

        public new IssueViewModel ViewModel
        {
            get { return (IssueViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public IssueView()
            : base(false)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.EditButton, () =>
            {
                var editController = new IssueEditView(ViewModel.Username, ViewModel.Repository)
                {
                    ExistingIssue = ViewModel.Issue,
                    Title = "Edit Issue",
                    Success = EditingComplete,
                };
                NavigationController.PushViewController(editController, true);
            }));
            NavigationItem.RightBarButtonItem.Enabled = false;

            ViewModel.Bind(x => x.Issue, RenderIssue);
            ViewModel.BindCollection(x => x.Comments, (e) => RenderComments());

            var path = System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "Issue.html");
            LoadFile(path);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            Title = "Issue #" + ViewModel.Id;
        }

        protected override void OnLoadError(object sender, UIWebErrorArgs e)
        {
            MonoTouch.Utilities.LogException(new Exception(e.Error.Description));
            MonoTouch.Utilities.ShowAlert("Error", e.Error.Description);
            base.OnLoadError(sender, e);
        }

        protected override bool ShouldStartLoad(NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (request.Url.AbsoluteString.StartsWith("codehub://ready"))
            {
                if (ViewModel.Issue != null)
                    RenderIssue();
                if (ViewModel.Comments.Items.Count > 0)
                    RenderComments();
            }
            else if (request.Url.AbsoluteString.StartsWith("codehub://add_comment"))
            {
                AddCommentTapped();
            }
            else if (request.Url.AbsoluteString.StartsWith("codehub://assignee/"))
            {
                var name = request.Url.AbsoluteString.Substring("codehub://assignee/".Length);
//                if (!string.IsNullOrEmpty(name))
//                    NavigationController.PushViewController(new ProfileViewController(name), true);
            }
            else if (request.Url.AbsoluteString.StartsWith("file"))
            {
                return true;
            }
            else if (request.Url.AbsoluteString.StartsWith("http"))
            {
                try { UIApplication.SharedApplication.OpenUrl(request.Url); } catch { }
            }

            return false;
        }

        public void RenderComments()
        {
//            var comments = ViewModel.Comments.Select(x => new { 
//                avatarUrl = x.User.AvatarUrl, 
//                login = x.User.Login, 
//                updated_at = x.CreatedAt.ToDaysAgo(), 
//                body = ViewModel.ConvertToMarkdown(x.Body)
//            });
//            var data = new RestSharp.Serializers.JsonSerializer().Serialize(comments.ToList());
//            Web.EvaluateJavascript("var a = " + data + "; setComments(a);");
        }

        public void RenderIssue()
        {
            NavigationItem.RightBarButtonItem.Enabled = true;

            var state = ViewModel.Issue.State;
            if (state == null)
                state = "No State";

            var milestone = ViewModel.Issue.Milestone;
            var milestoneStr = milestone != null ? milestone.Title : "No Milestone";
            var assignedTo = ViewModel.Issue.Assignee;
            var assignedToStr = ViewModel.Issue.Assignee != null ? ViewModel.Issue.Assignee.Login : null;

            var issue = new { state = state, 
                milestone = milestoneStr, 
                assigned_to = assignedToStr ?? "Unassigned", 
                updated_at = "Updated " + ViewModel.Issue.UpdatedAt.ToDaysAgo(),
                title = ViewModel.Issue.Title,
                assigned_to_login = assignedToStr ?? ""
            };

//            var data = new RestSharp.Serializers.JsonSerializer().Serialize(issue);
//            Web.EvaluateJavascript("var a = " + data + "; setData(a);");

            var md = new MarkdownSharp.Markdown();

            var desc = FileSourceViewController.JavaScriptStringEncode(md.Transform(ViewModel.Issue.Body));
            Web.EvaluateJavascript("var a = \"" + desc + "\"; setDescription(a);");
        }

        void EditingComplete(IssueModel model)
        {
            ViewModel.Issue = model;

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
//            var composer = new Composer();
//            composer.NewComment(this, (text) => {
//                try
//                {
//                    composer.DoWorkTest("Loading...".t(), async () => {
//                        await ViewModel.AddComment(text);
//                        composer.CloseComposer();
//                    });
//                }
//                catch (Exception ex)
//                {
//                    Utilities.ShowAlert("Unable to post comment!", ex.Message);
//                }
//                finally
//                {
//                    composer.EnableSendButton = true;
//                }
//            });
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


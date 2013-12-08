using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Issues;
using GitHubSharp.Models;
using MonoTouch.UIKit;
using CodeFramework.iOS.ViewControllers;
using MonoTouch.Dialog;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views.Issues
{
	public class IssueView : ViewModelDrivenViewController
    {
		private readonly HeaderView _header;
		private readonly SplitElement _split1;
        private bool _issueRemoved;

        public new IssueViewModel ViewModel
        {
            get { return (IssueViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public IssueView()
        {
			Root.UnevenRows = true;
			_header = new HeaderView() { ShadowImage = false };
			_split1 = new SplitElement(new SplitElement.Row { Image1 = Images.Cog, Image2 = Images.Milestone }) { BackgroundColor = UIColor.White };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.EditButton, () => ViewModel.GoToEditCommand.Execute(null)));
            NavigationItem.RightBarButtonItem.Enabled = false;
            ViewModel.Bind(x => x.Issue, RenderIssue);
            ViewModel.BindCollection(x => x.Comments, (e) => RenderComments());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            Title = "Issue #" + ViewModel.Id;
        }
//
//        protected override void OnLoadError(object sender, UIWebErrorArgs e)
//        {
//            MonoTouch.Utilities.LogException(new Exception(e.Error.Description));
//            MonoTouch.Utilities.ShowAlert("Error", e.Error.Description);
//            base.OnLoadError(sender, e);
//        }
//
//        protected override bool ShouldStartLoad(NSUrlRequest request, UIWebViewNavigationType navigationType)
//        {
//            if (request.Url.AbsoluteString.StartsWith("codehub://ready"))
//            {
//                if (ViewModel.Issue != null)
//                    RenderIssue();
//                if (ViewModel.Comments.Items.Count > 0)
//                    RenderComments();
//            }
//            else if (request.Url.AbsoluteString.StartsWith("codehub://add_comment"))
//            {
//                AddCommentTapped();
//            }
//            else if (request.Url.AbsoluteString.StartsWith("codehub://assignee/"))
//            {
//                var name = request.Url.AbsoluteString.Substring("codehub://assignee/".Length);
////                if (!string.IsNullOrEmpty(name))
////                    NavigationController.PushViewController(new ProfileViewController(name), true);
//            }
//            else if (request.Url.AbsoluteString.StartsWith("file"))
//            {
//                return true;
//            }
//            else if (request.Url.AbsoluteString.StartsWith("http"))
//            {
//                try { UIApplication.SharedApplication.OpenUrl(request.Url); } catch { }
//            }
//
//            return false;
//        }
//
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

			var root = new RootElement(Title);
			_header.Title = ViewModel.Issue.Title;
			_header.Subtitle = "Updated " + ViewModel.Issue.UpdatedAt.ToDaysAgo();
			root.Add(new Section(_header));

			var milestone = ViewModel.Issue.Milestone;
			var milestoneStr = milestone != null ? milestone.Title : "No Milestone";
			var secDetails = new Section();

			_split1.Value.Text1 = ViewModel.Issue.State ?? "No State";
			_split1.Value.Text2 = milestoneStr;
			secDetails.Add(_split1);

			var responsible = new StyledStringElement(ViewModel.Issue.Assignee != null ? ViewModel.Issue.Assignee.Login : "Unassigned".t()) {
				Font = StyledStringElement.DefaultDetailFont,
				TextColor = StyledStringElement.DefaultDetailColor,
				Image = Images.Person
			};

			if (ViewModel.Issue.Assignee != null)
			{
				responsible.Tapped += () => ViewModel.GoToAssigneeCommand.Execute(null);
				responsible.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			}

			secDetails.Add(responsible);
			root.Add(secDetails);

			var addComment = new StyledStringElement("Add Comment") { Image = Images.Pencil };
			addComment.Tapped += AddCommentTapped;
			root.Add(new Section { addComment });
			Root = root;
//
//            var issue = new { state = state, 
//                milestone = milestoneStr, 
//                assigned_to = assignedToStr ?? "Unassigned", 
//                updated_at = "Updated " + ViewModel.Issue.UpdatedAt.ToDaysAgo(),
//                title = ViewModel.Issue.Title,
//                assigned_to_login = assignedToStr ?? ""
//            };
//
//            var data = new RestSharp.Serializers.JsonSerializer().Serialize(issue);
//            Web.EvaluateJavascript("var a = " + data + "; setData(a);");

            var md = new MarkdownSharp.Markdown();

			//var desc = FileSourceViewController.JavaScriptStringEncode(md.Transform(ViewModel.Issue.Body));
			//Web.EvaluateJavascript("var a = \"" + desc + "\"; setDescription(a);");
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
			var composer = new Composer();
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


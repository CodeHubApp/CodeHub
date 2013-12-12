using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Issues;
using GitHubSharp.Models;
using MonoTouch.UIKit;
using CodeFramework.iOS.ViewControllers;
using MonoTouch.Dialog;
using CodeFramework.iOS.Utils;
using CodeFramework.iOS.Elements;
using System.Linq;

namespace CodeHub.iOS.Views.Issues
{
	public class IssueView : ViewModelDrivenViewController
    {
		private readonly HeaderView _header;
		private readonly SplitElement _split1;
		private WebElement _descriptionElement;
		private WebElement2 _commentsElement;


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

			var content = System.IO.File.ReadAllText("WebCell/body.html", System.Text.Encoding.UTF8);
			_descriptionElement = new WebElement(content);
			_descriptionElement.UrlRequested = ViewModel.GoToWeb.Execute;

			var content2 = System.IO.File.ReadAllText("WebCell/comments.html", System.Text.Encoding.UTF8);
			_commentsElement = new WebElement2(content2);
			_commentsElement.UrlRequested = ViewModel.GoToWeb.Execute;

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Compose, (s, e) => ViewModel.GoToEditCommand.Execute(null));
            NavigationItem.RightBarButtonItem.Enabled = false;
            ViewModel.Bind(x => x.Issue, RenderIssue);
            ViewModel.BindCollection(x => x.Comments, (e) => RenderComments());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            Title = "Issue #" + ViewModel.Id;
        }

        public void RenderComments()
        {
            var comments = ViewModel.Comments.Select(x => new { 
                avatarUrl = x.User.AvatarUrl, 
                login = x.User.Login, 
                updated_at = x.CreatedAt.ToDaysAgo(), 
                body = ViewModel.ConvertToMarkdown(x.Body)
            });

			var s = Cirrious.CrossCore.Mvx.Resolve<CodeFramework.Core.Services.IJsonSerializationService>();
			var data = s.Serialize(comments);
			InvokeOnMainThread(() => {
				if (_commentsElement.GetImmediateRootElement() == null)
					Root.Insert(Root.Count - 1, new Section() { _commentsElement });
				_commentsElement.Value = data;
			});
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

			if (!string.IsNullOrEmpty(ViewModel.Issue.Body))
			{
				_descriptionElement.Value = ViewModel.MarkdownDescription;
				secDetails.Add(_descriptionElement);
			}

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

			if (ViewModel.Issue.Comments > 0)
			{
				root.Add(new Section { _commentsElement });
			}

			var addComment = new StyledStringElement("Add Comment") { Image = Images.Pencil };
			addComment.Tapped += AddCommentTapped;
			root.Add(new Section { addComment });
			Root = root;
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

